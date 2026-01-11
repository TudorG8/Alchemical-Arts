using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(FluidCoordinatorSystem))]
	public partial struct DensityComputationSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			state.RequireForUpdate<FluidSimulationConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var fluidCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidCoordinatorSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();
			var fluidSimulationConstantsConfig = SystemAPI.GetSingleton<FluidSimulationConstantsConfig>();


			var computeDensitiesJob = new ComputeDensitiesJob()
			{
				densities = fluidCoordinatorSystem.densityBuffer,
				nearDensities = fluidCoordinatorSystem.nearDensityBuffer,
				spatial = fluidCoordinatorSystem.fluidSpatialBuffer,
				spatialOffsets = fluidCoordinatorSystem.fluidSpatialOffsetsBuffer,
				predictedPositions = spatialCoordinatorSystem.predictedPositionsBuffer,
				numParticles = fluidCoordinatorSystem.fluidCount,
				spatialPartioningConfig = spatialPartioningConfig,
				spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
				fluidSimulationConstantsConfig = fluidSimulationConstantsConfig,
				hashingLimit = spatialCoordinatorSystem.hashingLimit
			};
			handle = computeDensitiesJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, fluidCoordinatorSystem.handle);
		}
	}
}