using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	[UpdateAfter(typeof(FluidWritebackGroup))]
	public partial struct TemperatureTransferSystem : ISystem
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
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var temperatureCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var spatialPartioningConstantsConfig = SystemAPI.GetSingleton<SpatialPartioningConstantsConfig>();

			var transferTemperatureJob = new TransferTemperatureJob()
			{
				temperatureStateBuffer = temperatureCoordinatorSystem.temperatureStateBuffer,
				spatial = temperatureCoordinatorSystem.spatialBuffer,
				spatialOffsets = temperatureCoordinatorSystem.spatialOffsetsBuffer,
				predictedPositions = spatialCoordinatorSystem.predictedPositionsBuffer,
				numParticles = temperatureCoordinatorSystem.temperatureCount,
				spatialPartioningConfig = spatialPartioningConfig,
				spatialPartioningConstantsConfig = spatialPartioningConstantsConfig,
				hashingLimit = spatialCoordinatorSystem.hashingLimit
			};
			handle = transferTemperatureJob.ScheduleParallel(temperatureCoordinatorSystem.temperatureQuery, temperatureCoordinatorSystem.handle);
			handle.Complete();

			var x = "";
		}
	}
}