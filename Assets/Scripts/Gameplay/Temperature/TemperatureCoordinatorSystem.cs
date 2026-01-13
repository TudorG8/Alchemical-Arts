using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(SortJob<TemperatureSpatialEntry, TemperatureSpatialEntryComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<TemperatureSpatialEntry, TemperatureSpatialEntryComparer>.SegmentSortMerge))]
[assembly: RegisterGenericJobType(typeof(WritePartionedIndexJob<TemperaturePartionedIndex>))]

[UpdateInGroup(typeof(TemperatureGroup), OrderFirst = true)]
public partial struct TemperatureCoordinatorSystem : ISystem
{
	public int temperatureCount;

	public NativeArray<TemperatureSpatialEntry> spatialBuffer;
	
	public NativeArray<int> spatialOffsetsBuffer;

	public NativeArray<TemperatureState> temperatureStateBuffer;

	public EntityQuery temperatureQuery;

	public JobHandle handle;

	private int bufferCapacity;

	private ComponentTypeHandle<TemperaturePartionedIndex> temperatureIndexTypeHandle;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldState>();
		state.RequireForUpdate<SpatialPartioningConfig>();
		bufferCapacity = 10000;
		
		spatialBuffer = new NativeArray<TemperatureSpatialEntry>(bufferCapacity, Allocator.Persistent);
		spatialOffsetsBuffer = new NativeArray<int>(bufferCapacity, Allocator.Persistent);
		temperatureStateBuffer = new NativeArray<TemperatureState>(bufferCapacity, Allocator.Persistent);
		
		temperatureQuery =  SystemAPI.QueryBuilder().WithAll<TemperaturePartionedIndex>().WithAll<SpatiallyPartionedIndex>().WithAll<TemperatureState>().Build();
		temperatureIndexTypeHandle = state.GetComponentTypeHandle<TemperaturePartionedIndex>();
	}

	[BurstCompile]
	public void OnDestroy(ref SystemState state)
	{
		spatialBuffer.Dispose();
		spatialOffsetsBuffer.Dispose();
		temperatureStateBuffer.Dispose();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		temperatureCount = temperatureQuery.CalculateEntityCount();
		if (temperatureCount == 0)
			return;
		
		ref var spatialPartioningCoordinator = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
		var simulationConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();

		var temperatureStates = temperatureQuery.ToComponentDataArray<TemperatureState>(Allocator.Temp);
		NativeArray<TemperatureState>.Copy(temperatureStates, 0, temperatureStateBuffer, 0, temperatureCount);

		temperatureIndexTypeHandle.Update(ref state);
		var writeTemperaturePartionedHandle = PartitionedIndexJobUtility.ScheduleWritePartitionedIndex(temperatureQuery, temperatureIndexTypeHandle, state.Dependency);

		var buildFluidSpatialEntriesJob = new BuildTemperatureSpatialEntriesJob()
		{
			fluidSpatialBuffer = spatialBuffer,
			fluidSpatialOffsetsBuffer = spatialOffsetsBuffer,
			predictedPositionsBuffer = spatialPartioningCoordinator.predictedPositionsBuffer,
			radius = simulationConfig.radius,
			count = temperatureCount,
			hashingLimit = spatialPartioningCoordinator.hashingLimit
		};
		var buildFluidSpatialEntriesHandle = buildFluidSpatialEntriesJob.ScheduleParallel(temperatureQuery, writeTemperaturePartionedHandle);
		buildFluidSpatialEntriesHandle.Complete();


		var sortJobHandle = spatialBuffer.Slice(0, temperatureCount).SortJob(new TemperatureSpatialEntryComparer()).Schedule();


		var buildSpatialKeyOffsetsJob = new BuildTemperatureSpatialOffsetsJob()
		{
			spatial = spatialBuffer,
			spatialOffsets = spatialOffsetsBuffer
		};
		state.Dependency = handle = buildSpatialKeyOffsetsJob.Schedule(temperatureCount, 64, sortJobHandle);
	}
}