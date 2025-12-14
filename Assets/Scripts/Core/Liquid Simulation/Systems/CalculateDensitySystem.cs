using PotionCraft.Core.LiquidSimulation.Components;
using PotionCraft.Core.LiquidSimulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.LiquidSimulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(SpatialDataSystem))]
	partial struct CalculateDensitySystem : ISystem
	{
		public NativeArray<float> densities;

		public NativeArray<float> nearDensity;

		private SystemHandle spatialDataSystemHandle;

		private SystemHandle calculatePredictedPositionsSystemHandle;

		private SystemHandle populateLiquidPositionsSystemHandle;

		private NativeArray<int2> offsets2D;

		private float smoothingRadius;

		private float spikyPow3ScalingFactor;

		private float spikyPow2ScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldConfigComponent>();
			densities = new NativeArray<float>(10000, Allocator.Persistent);
			nearDensity = new NativeArray<float>(10000, Allocator.Persistent);
			spatialDataSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<SpatialDataSystem>();
			calculatePredictedPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<CalculatePredictedPositionsSystem>();
			populateLiquidPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();

			offsets2D = new NativeArray<int2>(9, Allocator.Persistent);
			offsets2D[0] = new int2(-1, 1);
			offsets2D[1] = new int2(0, 1);
			offsets2D[2] = new int2(1, 1);
			offsets2D[3] = new int2(-1, 0);
			offsets2D[4] = new int2(0, 0);
			offsets2D[5] = new int2(1, 0);
			offsets2D[6] = new int2(-1, -1);
			offsets2D[7] = new int2(0, -1);
			offsets2D[8] = new int2(1, -1);

			smoothingRadius = 0.35f;

			spikyPow3ScalingFactor = 10 / (math.PI * math.pow(smoothingRadius, 5));
			spikyPow2ScalingFactor = 6 / (math.PI * math.pow(smoothingRadius, 4));
		}

		[BurstCompile]
		public void OnDestroy()
		{
			densities.Dispose();
			nearDensity.Dispose();
			offsets2D.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialDataSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<SpatialDataSystem>(spatialDataSystemHandle);
			ref var calculatePredictedPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<CalculatePredictedPositionsSystem>(calculatePredictedPositionsSystemHandle);
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var calculateDensitiesJob = new CalculateDensitiesJob()
			{
				smoothingRadius = smoothingRadius,
				predictedPositions = calculatePredictedPositionsSystem.predictedPositionsBuffer,
				spatial = spatialDataSystem.Spatial,
				spatialOffsets = spatialDataSystem.SpatialOffsets,
				numParticles = populateLiquidPositionsSystem.count,
				offsets2D = offsets2D,
				SpikyPow2ScalingFactor = spikyPow2ScalingFactor,
				SpikyPow3ScalingFactor = spikyPow3ScalingFactor,
				densities = densities,
				nearDensities = nearDensity

			};
			var calculateDensitiesHandle = calculateDensitiesJob.ScheduleParallel(state.Dependency);
			calculateDensitiesHandle.Complete();
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		public partial struct CalculateDensitiesJob : IJobEntity 
		{
			[ReadOnly]
			public float smoothingRadius;

			[ReadOnly]
			public NativeArray<int> spatialOffsets;

			[ReadOnly]
			public NativeArray<SpatialEntry> spatial;

			[ReadOnly]
			public NativeArray<float2> predictedPositions;

			[ReadOnly]
			public int numParticles;

			[ReadOnly]
			public NativeArray<int2> offsets2D;

			[ReadOnly]
			public float SpikyPow3ScalingFactor;

			[ReadOnly]
			public float SpikyPow2ScalingFactor;

			[WriteOnly]
			public NativeArray<float> densities;

			[WriteOnly]
			public NativeArray<float> nearDensities;


			void Execute(
				[EntityIndexInQuery] int index)
			{
				var pos = predictedPositions[index];
				var originCell = GetCell2D(pos, smoothingRadius);
				var sqrRadius = smoothingRadius * smoothingRadius;
				var density = 0f;
				var nearDensity = 0f;
				
				for(int i = 0; i < 9; i++)
				{
					var hash = HashCell2D(originCell + offsets2D[i]);
					var key = (int)KeyFromHash(hash, numParticles);
					var currIndex = spatialOffsets[key];

					while (currIndex < numParticles)
					{
						var neighbourIndex = currIndex;
						var test = spatial[neighbourIndex].index;
						currIndex++;

						var neighbourKey = spatial[neighbourIndex].key;
						if (neighbourKey != key) break;

						var neighbourPos = predictedPositions[test];
						var offsetToNeighbour = neighbourPos - pos;
						var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

						if (sqrDstToNeighbour > sqrRadius) continue;

						var dst = math.sqrt(sqrDstToNeighbour);
						density += DensityKernel(dst, smoothingRadius);
						nearDensity += NearDensityKernel(dst, smoothingRadius);
					}
				}

				densities[index] = density;
				nearDensities[index] = nearDensity;
			}

			float SpikyKernelPow3(float dst, float radius)
			{
				if (dst < radius)
				{
					float v = radius - dst;
					return v * v * v * SpikyPow3ScalingFactor;
				}
				return 0;
			}

			float SpikyKernelPow2(float dst, float radius)
			{
				if (dst < radius)
				{
					float v = radius - dst;
					return v * v * SpikyPow2ScalingFactor;
				}
				return 0;
			}

			float DensityKernel(float dst, float radius)
			{
				return SpikyKernelPow2(dst, radius);
			}

			float NearDensityKernel(float dst, float radius)
			{
				return SpikyKernelPow3(dst, radius);
			}

			public uint HashCell2D(float2 input)
			{
				var a = (uint)input.x * 15823;
				var b = (uint)input.y * 9737333;
				return a + b;
			}

			public int2 GetCell2D(float2 input, float radius)
			{
				var x = (int)math.floor(input.x / radius);
				var y = (int)math.floor(input.y / radius);

				return new int2(x, y);
			}

			public readonly uint KeyFromHash(uint hash, int length)
			{
				return hash % (uint)length;
			}
		}
	}
}