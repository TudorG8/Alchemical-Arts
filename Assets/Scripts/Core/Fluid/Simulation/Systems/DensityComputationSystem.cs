using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	public partial struct DensityComputationSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();
			var fluidSimulationConstantsConfig = SystemAPI.GetSingleton<FluidSimulationConstantsConfig>();

			var computeDensitiesJob = new ComputeDensitiesJob()
			{
				densities = fluidBuffersSystem.densityBuffer,
				nearDensities = fluidBuffersSystem.nearDensityBuffer,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				numParticles = fluidBuffersSystem.count,
				spatialPartioningConfig = spatialPartioningConfig,
				spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
				fluidSimulationConstantsConfig = fluidSimulationConstantsConfig,
				hashingLimit = fluidBuffersSystem.hashingLimit
			};
			handle = computeDensitiesJob.ScheduleParallel(state.Dependency);
		}
	}
}