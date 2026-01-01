using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(PressureForceSystem))]
	partial struct ViscosityForceSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
		}


		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();
			var simulationConstantsState = SystemAPI.GetSingleton<SimulationConstantsState>();

			var applyViscosityForcesJob = new ApplyViscosityForcesJob()
			{
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				spatial = spatialPartitioningSystem.Spatial,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				numParticles = fluidPositionInitializationSystem.count,
				simulationState = simulationState,
				simulationConstantsState = simulationConstantsState,
				deltaTime = SystemAPI.Time.DeltaTime,
				hashingLimit = 10000,
			};
			handle = applyViscosityForcesJob.ScheduleParallel(pressureForceSystem.handle);
		}
	}
}