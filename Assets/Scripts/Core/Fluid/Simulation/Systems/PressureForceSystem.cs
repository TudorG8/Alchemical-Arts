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
	[UpdateAfter(typeof(FluidInwardsInputSystem))]
	partial struct PressureForceSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationState>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var densityComputationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<DensityComputationSystem>();
			ref var fluidInwardsInputSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidInwardsInputSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();
			var simulationConstantsState = SystemAPI.GetSingleton<SimulationConstantsState>();

			var applyPressureForcesJob = new ApplyPressureForcesJob()
			{
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				spatial = spatialPartitioningSystem.Spatial,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				densities = densityComputationSystem.densities,
				nearDensity = densityComputationSystem.nearDensity,
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				numParticles = fluidPositionInitializationSystem.count,
				simulationState = simulationState,
				simulationConstantsState = simulationConstantsState,
				deltaTime = SystemAPI.Time.DeltaTime,
				hashingLimit = 10000
			};
			handle = applyPressureForcesJob.ScheduleParallel(fluidInwardsInputSystem.handle);
		}
	}
}