using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(CalculateDensitySystem))]
	partial struct PressureForceSystem : ISystem
	{
		private NativeArray<int2> offsets2D;

		private SystemHandle spatialDataSystem;
		
		private SystemHandle positionSystem;

		private SystemHandle predictedPositionsSystem;

		private SystemHandle densitySystem;

		private float smoothingRadius;

		private float targetDensity;

		private float pressureMultiplier;

		private float nearPressureMultiplier;

		private float spikyPow2DerivativeScalingFactor;

		private float spikyPow3DerivativeScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();

			spatialDataSystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<SpatialDataSystem>();
			predictedPositionsSystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<CalculatePredictedPositionsSystem>();
			densitySystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<CalculateDensitySystem>();
			positionSystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();

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
			targetDensity = 55f;
			pressureMultiplier = 500f;
			nearPressureMultiplier = 4f;
			spikyPow2DerivativeScalingFactor = 12 / (math.pow(smoothingRadius, 4) * math.PI);
			spikyPow3DerivativeScalingFactor = 30 / (math.pow(smoothingRadius, 5) * math.PI);
		}

		[BurstCompile]
		public void OnDestroy()
		{
			offsets2D.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialDataSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<SpatialDataSystem>(this.spatialDataSystem);
			ref var calculatePredictedPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<CalculatePredictedPositionsSystem>(predictedPositionsSystem);
			ref var calculateDensitySystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<CalculateDensitySystem>(densitySystem);
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(positionSystem);

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var applyPressureForceJob = new ApplyPressureForceJob()
			{
				densities = calculateDensitySystem.densities,
				nearDensity = calculateDensitySystem.nearDensity,
				predictedPositions = calculatePredictedPositionsSystem.predictedPositionsBuffer,
				spatialOffsets = spatialDataSystem.SpatialOffsets,
				spatial = spatialDataSystem.Spatial,
				smoothingRadius = smoothingRadius,
				offsets2D = offsets2D,
				numParticles = populateLiquidPositionsSystem.count,
				deltaTime = SystemAPI.Time.DeltaTime,
				targetDensity = targetDensity,
				pressureMultiplier = pressureMultiplier,
				nearPressureMultiplier = nearPressureMultiplier,
				spikyPow2DerivativeScalingFactor = spikyPow2DerivativeScalingFactor,
				spikyPow3DerivativeScalingFactor = spikyPow3DerivativeScalingFactor,
				velocities = populateLiquidPositionsSystem.velocityBuffer
			};
			var applyPressureHandle = applyPressureForceJob.ScheduleParallel(state.Dependency);
			applyPressureHandle.Complete();
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct ApplyPressureForceJob : IJobEntity
		{
			[ReadOnly]
			public NativeArray<float> densities;

			[ReadOnly]
			public NativeArray<float> nearDensity;

			[ReadOnly]
			public NativeArray<float2> predictedPositions;

			[ReadOnly]
			public NativeArray<int> spatialOffsets;

			[ReadOnly]
			public NativeArray<SpatialEntry> spatial;

			[ReadOnly]
			public float smoothingRadius;

			[ReadOnly]
			public NativeArray<int2> offsets2D;

			[ReadOnly]
			public int numParticles;

			[ReadOnly]
			public float deltaTime;

			[ReadOnly]
			public float targetDensity;

			[ReadOnly]
			public float pressureMultiplier;

			[ReadOnly]
			public float nearPressureMultiplier;

			[ReadOnly]
			public float spikyPow2DerivativeScalingFactor;

			[ReadOnly]
			public float spikyPow3DerivativeScalingFactor;

			public NativeArray<float2> velocities;


			void Execute(
				[EntityIndexInQuery] int index)
			{
				var density = densities[index];
				var densityNear = nearDensity[index];
				var pressure = PressureFromDensity(density);
				var nearPressure = NearPressureFromDensity(densityNear);
				var pressureForce = new float2();

				var pos = predictedPositions[index];
				var originCell = GetCell2D(pos, smoothingRadius);
				var sqrRadius = smoothingRadius * smoothingRadius;
				
				for(int i = 0; i < 9; i++)
				{
					var hash = HashCell2D(originCell + offsets2D[i]);
					var key = (int)KeyFromHash(hash, 10000);
					var currIndex = spatialOffsets[key];
					while (currIndex < numParticles)
					{
						var neighbourIndex = currIndex;
						var test = spatial[neighbourIndex].index;
						currIndex++;

						if (test == index) continue;

						var neighbourKey = spatial[neighbourIndex].key;
						if (neighbourKey != key) break;

						var neighbourPos = predictedPositions[test];
						var offsetToNeighbour = neighbourPos - pos;
						var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

						if (sqrDstToNeighbour > sqrRadius) continue;

						var dst = math.sqrt(sqrDstToNeighbour);
						var dirToNeighbour = dst > 0 ? offsetToNeighbour / dst : new float2(0, 1);
						
						var neighbourDensity = densities[test];
						var neighbourNearDensity = nearDensity[test];
						var neighbourPressure = PressureFromDensity(neighbourDensity);
						var neighbourNearPressure = NearPressureFromDensity(neighbourNearDensity);
						
						var sharedPressure = (pressure + neighbourPressure) * 0.5f;
						var sharedNearPressure = (nearPressure + neighbourNearPressure) * 0.5f;
						
						pressureForce += dirToNeighbour * DensityDerivative(dst, smoothingRadius) * sharedPressure / neighbourDensity;
						pressureForce += dirToNeighbour * NearDensityDerivative(dst, smoothingRadius) * sharedNearPressure / neighbourNearDensity;
					}
				}

				var acceleration = pressureForce / density;
				velocities[index] += acceleration * deltaTime;
			}

			float DerivativeSpikyPow2(float dst, float radius)
			{
				if (dst <= radius)
				{
					float v = radius - dst;
					return -v * spikyPow2DerivativeScalingFactor;
				}
				return 0;
			}

			float DerivativeSpikyPow3(float dst, float radius)
			{
				if (dst <= radius)
				{
					float v = radius - dst;
					return -v * v * spikyPow3DerivativeScalingFactor;
				}
				return 0;
			}

			float DensityDerivative(float dst, float radius)
			{
				return DerivativeSpikyPow2(dst, radius);
			}

			float NearDensityDerivative(float dst, float radius)
			{
				return DerivativeSpikyPow3(dst, radius);
			}
			
			float PressureFromDensity(float density)
			{
				return (density - targetDensity) * pressureMultiplier;
			}

			float NearPressureFromDensity(float nearDensity)
			{
				return nearPressureMultiplier * nearDensity;
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