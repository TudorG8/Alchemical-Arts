using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Groups;
using PotionCraft.Core.SpatialPartioning.Jobs;
using PotionCraft.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSortMerge))]

namespace PotionCraft.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	[UpdateAfter(typeof(PositionPredictionSystem))]
	public partial struct SpatialPartitioningSystem : ISystem
	{
		public JobHandle handle;


		private SpatialEntryKeyComparer spatialEntryKeyComparer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			spatialEntryKeyComparer = new SpatialEntryKeyComparer();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();

			var buildSpatialEntriesJob = new BuildSpatialEntriesJob
			{
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				count = fluidBuffersSystem.count,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				hashingLimit = fluidBuffersSystem.hashingLimit
			};
			var buildSpatialEntriesHandle = buildSpatialEntriesJob.ScheduleParallel(positionPredictionSystem.handle);
			buildSpatialEntriesHandle.Complete(); // SortJob won't work without completing this

			var sortJobHandle = fluidBuffersSystem.spatialBuffer.Slice(0, fluidBuffersSystem.count).SortJob(spatialEntryKeyComparer).Schedule();

			var buildSpatialKeyOffsetsJob = new BuildSpatialKeyOffsetsJob()
			{
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer
			};
			handle = buildSpatialKeyOffsetsJob.ScheduleParallel(sortJobHandle);
			handle.Complete(); // Completing before we start doing actual work with forces
		}
	}
}