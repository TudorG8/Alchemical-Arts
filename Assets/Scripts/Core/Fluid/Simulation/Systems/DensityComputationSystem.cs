using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(SpatialPartitioningSystem))]
	partial struct DensityComputationSystem : ISystem
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
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidBuffersSystem>();
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();
			var simulationConstantsState = SystemAPI.GetSingleton<SimulationConstantsState>();

			var computeDensitiesJob = new ComputeDensitiesJob()
			{
				densities = fluidBuffersSystem.densityBuffer,
				nearDensities = fluidBuffersSystem.nearDensityBuffer,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				numParticles = fluidBuffersSystem.count,
				simulationState = simulationState,
				simulationConstantsState = simulationConstantsState,
				hashingLimit = fluidBuffersSystem.hashingLimit
			};
			handle = computeDensitiesJob.ScheduleParallel(spatialPartitioningSystem.handle);
		}
	}
}