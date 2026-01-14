using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSortMerge))]

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	[UpdateAfter(typeof(PositionPredictionSystem))]
	public partial struct SpatialPartioningSystem : ISystem
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
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();

			var buildSpatialEntriesJob = new BuildSpatialEntriesJob
			{
				spatial = spatialCoordinatorSystem.spatialBuffer,
				spatialOffsets = spatialCoordinatorSystem.spatialOffsetsBuffer,
				predictedPositions = spatialCoordinatorSystem.predictedPositionsBuffer,
				radius = spatialPartioningConfig.radius,
				count = spatialCoordinatorSystem.count,
				hashingLimit = spatialCoordinatorSystem.hashingLimit
			};
			handle = buildSpatialEntriesJob.ScheduleParallel(spatialCoordinatorSystem.simulatedQuery, positionPredictionSystem.handle);
		}
	}
}