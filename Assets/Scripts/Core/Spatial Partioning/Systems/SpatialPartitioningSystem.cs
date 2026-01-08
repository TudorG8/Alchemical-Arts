using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSortMerge))]

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	[UpdateAfter(typeof(PositionPredictionSystem))]
	public partial struct SpatialPartitioningSystem : ISystem
	{
		public JobHandle handle;


		private SpatialEntryKeyComparer spatialEntryKeyComparer;

		private EntityQuery spatialQuery;

		private EntityQuery fluidQuery;

		private ComponentLookup<FluidItemTag> fluidLookup;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			spatialEntryKeyComparer = new SpatialEntryKeyComparer();
			spatialQuery = SystemAPI.QueryBuilder()
				.WithAll<SpatiallyPartionedItemState>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
			fluidQuery =  SystemAPI.QueryBuilder().WithAll<FluidItemTag>().Build();
			fluidLookup = SystemAPI.GetComponentLookup<FluidItemTag>();
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
				predictedPositionsBuffer = fluidBuffersSystem.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				count = fluidBuffersSystem.count,
				spatial = fluidBuffersSystem.spatialBuffer,
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer,
				hashingLimit = fluidBuffersSystem.hashingLimit
			};
			var buildSpatialEntriesHandle = buildSpatialEntriesJob.ScheduleParallel(positionPredictionSystem.handle);
			buildSpatialEntriesHandle.Complete(); // SortJob won't work without completing this

			var sortJobHandle = fluidBuffersSystem.spatialBuffer.Slice(0, fluidBuffersSystem.count).SortJob(spatialEntryKeyComparer).Schedule();
			sortJobHandle.Complete();

			fluidLookup.Update(ref state);
			fluidBuffersSystem.fluidSpatialBuffer.Clear();
			var spatialEntities = spatialQuery.ToEntityArray(Allocator.TempJob);
			var buildFluidSpatialDataJob = new BuildFluidSpatialData()
			{
				spatialBuffer = fluidBuffersSystem.spatialBuffer,
				spatialEntities = spatialEntities,
				fluidSpatialBuffer = fluidBuffersSystem.fluidSpatialBuffer,
				componentLookup = fluidLookup,
				count = fluidBuffersSystem.count
			};
			var buildFluidSpatialDataHandle = buildFluidSpatialDataJob.Schedule(sortJobHandle);
			buildFluidSpatialDataHandle.Complete();

			var buildSpatialKeyOffsetsJob = new BuildSpatialKeyOffsetsJob()
			{
				spatial = fluidBuffersSystem.fluidSpatialBuffer.AsArray(),
				spatialOffsets = fluidBuffersSystem.spatialOffsetsBuffer
			};
			handle = buildSpatialKeyOffsetsJob.Schedule(fluidBuffersSystem.fluidCount, 64, buildFluidSpatialDataHandle);
			handle.Complete(); // Completing before we start doing actual work with forces

			var x = 0;
		}

		[BurstCompile]
		public partial struct BuildFluidSpatialData : IJob
		{
			public int count;

			[ReadOnly]
			public NativeArray<SpatialEntry> spatialBuffer;
			
			[ReadOnly]
			[DeallocateOnJobCompletion]
			public NativeArray<Entity> spatialEntities;

			[WriteOnly]
			public NativeList<FluidSpatialEntry> fluidSpatialBuffer;

			[ReadOnly]
			public ComponentLookup<FluidItemTag> componentLookup;


			public void Execute()
			{
				for (int i = 0; i < count; i++)
				{
					if (!componentLookup.HasComponent(spatialBuffer[i].entity))
						continue;

					var fluidIndex = componentLookup[spatialBuffer[i].entity];

					fluidSpatialBuffer.AddNoResize(new FluidSpatialEntry() { key = spatialBuffer[i].key, simulationIndex = spatialBuffer[i].simulationIndex, fluidIndex = fluidIndex.index});
				}
			}
		}
	}
}