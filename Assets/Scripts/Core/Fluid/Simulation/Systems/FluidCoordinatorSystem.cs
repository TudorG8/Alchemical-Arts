using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Fluid.Simulation.Groups;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
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
	[UpdateInGroup(typeof(FluidPhysicsGroup), OrderFirst = true)]
	public partial struct FluidCoordinatorSystem : ISystem
	{
		public int fluidCount;

		public NativeArray<FluidSpatialEntry> fluidSpatialBuffer;
		
		public NativeArray<int> fluidSpatialOffsetsBuffer;

		public NativeArray<float> densityBuffer;

		public NativeArray<float> nearDensityBuffer;

		public NativeList<int> inwardsForceBuffer;

		public NativeArray<BatchVelocity> batchVelocityBuffer;

		public EntityQuery fluidQuery;

		public JobHandle handle;

		private int bufferCapacity;

		private ComponentTypeHandle<FluidPartionedIndex> fluidIndexTypeHandle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			bufferCapacity = 10000;
			
			fluidSpatialBuffer = new NativeArray<FluidSpatialEntry>(bufferCapacity, Allocator.Persistent);
			fluidSpatialOffsetsBuffer = new NativeArray<int>(bufferCapacity, Allocator.Persistent);
			densityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			nearDensityBuffer = new NativeArray<float>(bufferCapacity, Allocator.Persistent);
			inwardsForceBuffer = new NativeList<int>(bufferCapacity, Allocator.Persistent);
			batchVelocityBuffer = new NativeArray<BatchVelocity>(bufferCapacity, Allocator.Persistent);

			fluidQuery =  SystemAPI.QueryBuilder().WithAll<FluidPartionedIndex>().WithAll<SpatiallyPartionedIndex>().Build();
			fluidIndexTypeHandle = state.GetComponentTypeHandle<FluidPartionedIndex>();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			fluidSpatialBuffer.Dispose();
			fluidSpatialOffsetsBuffer.Dispose();
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
			var writeFluidPartionedIndexHandle = PartitionedIndexJobUtility.ScheduleWritePartitionedIndex(fluidQuery, fluidIndexTypeHandle, state.Dependency);

			var buildFluidSpatialEntriesJob = new BuildFluidSpatialEntriesJob()
			{
				fluidSpatialBuffer = fluidSpatialBuffer,
				fluidSpatialOffsetsBuffer = fluidSpatialOffsetsBuffer,
				predictedPositionsBuffer = spatialPartioningCoordinator.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				count = fluidCount,
				hashingLimit = spatialPartioningCoordinator.hashingLimit
			};
			var buildFluidSpatialEntriesHandle = buildFluidSpatialEntriesJob.ScheduleParallel(fluidQuery, writeFluidPartionedIndexHandle);
			buildFluidSpatialEntriesHandle.Complete();

			var sortJobHandle = fluidSpatialBuffer.Slice(0, fluidCount).SortJob(new FluidSpatialEntryComparer()).Schedule();

			var buildSpatialKeyOffsetsJob = new BuildSpatialOffsetsJob<FluidSpatialEntry>()
			{
				spatial = fluidSpatialBuffer,
				spatialOffsets = fluidSpatialOffsetsBuffer
			};
			handle = buildSpatialKeyOffsetsJob.Schedule(fluidCount, 64, sortJobHandle);
		}
	}
}