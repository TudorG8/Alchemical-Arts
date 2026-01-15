using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Fluid.Simulation.Systems;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

[assembly: RegisterGenericJobType(typeof(SortJob<FluidSpatialEntry, FluidSpatialEntryComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<FluidSpatialEntry, FluidSpatialEntryComparer>.SegmentSortMerge))]
[assembly: RegisterGenericJobType(typeof(WritePartionedIndexJob<FluidPartionedIndex>))]
[assembly: RegisterGenericJobType(typeof(BuildSpatialOffsetsJob<FluidSpatialEntry>))]

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	[UpdateAfter(typeof(SpatialPartioningSystem))]
	public partial struct FluidCoordinatorSystem : ISystem
	{
		public int fluidCount;

		public NativeArray<FluidSpatialEntry> spatialBuffer;
		
		public NativeArray<int> spatialOffsetsBuffer;

		public NativeArray<float> densityBuffer;

		public NativeArray<float> nearDensityBuffer;

		public NativeList<int> inwardsForceBuffer;

		public NativeArray<BatchVelocity> batchVelocityBuffer;

		public EntityQuery fluidQuery;

		public JobHandle handle;


		private int bufferCapacity;

		private ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexTypeHandle;

		private ComponentTypeHandle<FluidPartionedIndex> fluidIndexTypeHandle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			bufferCapacity = 10000;
			
			spatialBuffer = new NativeArray<FluidSpatialEntry>(bufferCapacity, Allocator.Persistent);
			spatialOffsetsBuffer = new NativeArray<int>(bufferCapacity, Allocator.Persistent);
			densityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			nearDensityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			inwardsForceBuffer = new NativeList<int>(bufferCapacity, Allocator.Persistent);
			batchVelocityBuffer = new NativeArray<BatchVelocity>(bufferCapacity, Allocator.Persistent);

			fluidQuery =  SystemAPI.QueryBuilder().WithAll<FluidPartionedIndex>().WithAll<SpatiallyPartionedIndex>().Build();
			fluidIndexTypeHandle = state.GetComponentTypeHandle<FluidPartionedIndex>(isReadOnly: false);
			spatialIndexTypeHandle = state.GetComponentTypeHandle<SpatiallyPartionedIndex>(isReadOnly: true);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			spatialBuffer.Dispose();
			spatialOffsetsBuffer.Dispose();
			densityBuffer.Dispose();
			nearDensityBuffer.Dispose();
			inwardsForceBuffer.Dispose();
			batchVelocityBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			fluidCount = fluidQuery.CalculateEntityCount();
			if (fluidCount == 0)
				return;

			ref var spatialPartioningCoordinator = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			var simulationConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();

			fluidIndexTypeHandle.Update(ref state);
			spatialIndexTypeHandle.Update(ref state);
			var writeFluidPartionedIndexHandle = PartitionedIndexJobUtility.ScheduleWritePartitionedIndex(fluidQuery, fluidIndexTypeHandle, state.Dependency);

			var calculateBaseEntityIndexHandle = fluidQuery.CalculateBaseEntityIndexArrayAsync(Allocator.TempJob, writeFluidPartionedIndexHandle, out var indexHandle);
			var buildSpatialEntriesJob = new BuildSpatialEntriesWithIndexJob<FluidSpatialEntry, FluidPartionedIndex>()
			{
				entityIndexes = calculateBaseEntityIndexHandle,
				spatialBuffer = spatialBuffer,
				spatialOffsetsBuffer = spatialOffsetsBuffer,
				predictedPositionsBuffer = spatialPartioningCoordinator.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				hashingLimit = spatialPartioningCoordinator.hashingLimit,
				spatialIndexHandle = spatialIndexTypeHandle,
				componentIndexHandle = fluidIndexTypeHandle,
			};
			handle = buildSpatialEntriesJob.ScheduleParallel(fluidQuery, indexHandle);
		}
	}
}