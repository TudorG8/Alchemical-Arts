using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(PressureForceSystem))]
	partial struct ViscositySystem : ISystem
	{
		public JobHandle handle;


		private NativeArray<int2> offsets2D;

		private float poly6ScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();

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

			poly6ScalingFactor = 4 / (math.PI * math.pow(0.35f, 8));
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PopulateLiquidPositionsSystem>();
			ref var calculatePredictedPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<CalculatePredictedPositionsSystem>();
			ref var spatialDataSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialDataSystem>();
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var viscosityJob = new ViscosityJob()
			{
				predictedPositions = calculatePredictedPositionsSystem.predictedPositionsBuffer,
				spatialOffsets = spatialDataSystem.SpatialOffsets,
				spatial = spatialDataSystem.Spatial,
				smoothingRadius = simulationConfig.radius,
				offsets2D = offsets2D,
				numParticles = populateLiquidPositionsSystem.count,
				deltaTime = SystemAPI.Time.DeltaTime,
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				poly6ScalingFactor = poly6ScalingFactor,
				viscosityStrength = simulationConfig.viscosityStrength,
			};
			handle = viscosityJob.ScheduleParallel(pressureForceSystem.handle);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			offsets2D.Dispose();
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct ViscosityJob : IJobEntity
		{
			[ReadOnly]
			public NativeArray<float2> predictedPositions;

			[ReadOnly]
			public NativeArray<int> spatialOffsets;

			[ReadOnly]
			public NativeArray<SpatialEntry> spatial;

			[ReadOnly]
			public NativeArray<int2> offsets2D;

			[ReadOnly]
			public float smoothingRadius;

			[ReadOnly]
			public int numParticles;

			[ReadOnly]
			public float poly6ScalingFactor;

			[ReadOnly]
			public float viscosityStrength;

			[ReadOnly]
			public float deltaTime;

			[NativeDisableParallelForRestriction]
			public NativeArray<float2> velocities;


			void Execute(
				[EntityIndexInQuery] int input)
			{
				var pos = predictedPositions[input];
				var originCell = GetCell2D(pos, smoothingRadius);
				var sqrRadius = smoothingRadius * smoothingRadius;

				var viscosityForce = float2.zero;
				var velocity = velocities[input];

				for (int i = 0; i < 9; i ++)
				{
					var hash = HashCell2D(originCell + offsets2D[i]);
					var key = (int)KeyFromHash(hash, 10000);
					var currIndex = spatialOffsets[key];

					while (currIndex < numParticles)
					{
						var spatialNeighbourIndex = currIndex;
						var neighbourIndex = spatial[spatialNeighbourIndex].index;
						currIndex ++;

						if (neighbourIndex == input) continue;
						
						var neighbourKey = spatial[spatialNeighbourIndex].key;
						if (neighbourKey != key) break;

						var neighbourPos = predictedPositions[neighbourIndex];
						var offsetToNeighbour = neighbourPos - pos;
						var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

						if (sqrDstToNeighbour > sqrRadius) continue;

						var dst = math.sqrt(sqrDstToNeighbour);
						var neighbourVelocity = velocities[neighbourIndex];
						viscosityForce += (neighbourVelocity - velocity) * ViscosityKernel(dst, smoothingRadius);
					}

				}
				velocities[input] += deltaTime * viscosityStrength * viscosityForce;
			}

			float ViscosityKernel(float dst, float radius)
			{
				return SmoothingKernelPoly6(dst, radius);
			}

			float SmoothingKernelPoly6(float dst, float radius)
			{
				if (dst < radius)
				{
					float v = radius * radius - dst * dst;
					return v * v * v * poly6ScalingFactor;
				}
				return 0;
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