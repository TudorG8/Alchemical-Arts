using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(DensityComputationSystem))]
	partial struct PressureForceSystem : ISystem
	{
		public JobHandle handle;


		private NativeArray<int2> offsets2D;

		private float spikyPow2DerivativeScalingFactor;

		private float spikyPow3DerivativeScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();

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

			spikyPow2DerivativeScalingFactor = 12 / (math.pow(0.35f, 4) * math.PI);
			spikyPow3DerivativeScalingFactor = 30 / (math.pow(0.35f, 5) * math.PI);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			offsets2D.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var densityComputationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<DensityComputationSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var applyPressureForcesJob = new ApplyPressureForcesJob()
			{
				densities = densityComputationSystem.densities,
				nearDensity = densityComputationSystem.nearDensity,
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				spatial = spatialPartitioningSystem.Spatial,
				smoothingRadius = simulationConfig.radius,
				offsets2D = offsets2D,
				numParticles = fluidPositionInitializationSystem.count,
				deltaTime = SystemAPI.Time.DeltaTime,
				targetDensity = simulationConfig.targetDensity,
				pressureMultiplier = simulationConfig.pressureMultiplier,
				nearPressureMultiplier = simulationConfig.nearPressureMultiplier,
				spikyPow2DerivativeScalingFactor = spikyPow2DerivativeScalingFactor,
				spikyPow3DerivativeScalingFactor = spikyPow3DerivativeScalingFactor,
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				hashingLimit = 10000
			};
			handle = applyPressureForcesJob.ScheduleParallel(densityComputationSystem.handle);
		}
	}
}