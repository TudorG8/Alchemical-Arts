using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Gameplay.Temperature.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(SortSpatialEntriesJob<TemperatureSpatialEntry, TemperatureSpatialEntryComparer>))]
[assembly: RegisterGenericJobType(typeof(BuildSpatialOffsetsJob<TemperatureSpatialEntry>))]

namespace AlchemicalArts.Gameplay.Temperature.Systems
{
	[UpdateInGroup(typeof(SpatialSortingGroup))]
	public partial struct TemperatureSpatialSortingSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<SpatialPartioningConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var temperatureCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
			if (temperatureCoordinatorSystem.temperatureCount == 0)
				return;


			var sortSpatialEntriesJob = new SortSpatialEntriesJob<TemperatureSpatialEntry, TemperatureSpatialEntryComparer>()
			{
				spatial = temperatureCoordinatorSystem.spatialBuffer,
				spatialComparer = new TemperatureSpatialEntryComparer(),
				count = spatialCoordinatorSystem.count,
			};
			var sortSpatialEntriesHandle = sortSpatialEntriesJob.Schedule(temperatureCoordinatorSystem.handle);

			var buildSpatialKeyOffsetsJob = new BuildSpatialOffsetsJob<TemperatureSpatialEntry>()
			{
				spatial = temperatureCoordinatorSystem.spatialBuffer,
				spatialOffsets = temperatureCoordinatorSystem.spatialOffsetsBuffer
			};
			handle = buildSpatialKeyOffsetsJob.Schedule(temperatureCoordinatorSystem.temperatureCount, 64, sortSpatialEntriesHandle);
			state.Dependency = handle;
		}
	}
}