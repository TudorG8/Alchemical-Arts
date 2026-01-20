using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Gameplay.Temperature.Components;
using AlchemicalArts.Gameplay.Temperature.Groups;
using AlchemicalArts.Gameplay.Temperature.Jobs;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace AlchemicalArts.Gameplay.Temperature.Systems
{
	[UpdateInGroup(typeof(TemperatureGroup))]
	public partial struct TemperatureTransferSystem : ISystem
	{
		private const int FRAME_COUNT = 5;

		public JobHandle handle;

		private int batch;

		private EntityQuery transmissionQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpatialPartioningConfig>();
			batch = 0;

			transmissionQuery = SystemAPI.QueryBuilder().WithPresent<TemperatureEnabledTransmissionTag>().Build();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var temperatureCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
			ref var temperatureSpatialSortingSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureSpatialSortingSystem>();
			if (temperatureCoordinatorSystem.temperatureCount == 0)
				return;

			var entitiesToProcess = math.min(temperatureCoordinatorSystem.temperatureCount / FRAME_COUNT, 2000);
			var startIndex = batch * entitiesToProcess;
			var endIndex = math.min(startIndex + entitiesToProcess, temperatureCoordinatorSystem.temperatureCount);

			state.EntityManager.SetComponentEnabled<TemperatureEnabledTransmissionTag>(transmissionQuery, false);
			var entities = transmissionQuery.ToEntityArray(state.WorldUpdateAllocator);

			for (int i = startIndex; i < endIndex; i++)
			{
				state.EntityManager.SetComponentEnabled<TemperatureEnabledTransmissionTag>(entities[i], true);
			}
			
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
			handle = transferTemperatureJob.ScheduleParallel(temperatureSpatialSortingSystem.handle);
			state.Dependency = handle;

			batch = (batch + 1) % FRAME_COUNT;
		}
	}
}