using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(WritePartionedIndexJob<TemperaturePartionedIndex>))]
[assembly: RegisterGenericJobType(typeof(BuildSpatialEntriesWithIndexJob<TemperatureSpatialEntry, TemperaturePartionedIndex>))]

[UpdateInGroup(typeof(SpatialPartioningGroup))]
[UpdateAfter(typeof(SpatialPartioningSystem))]
public partial struct TemperatureCoordinatorSystem : ISystem
{
	public int temperatureCount;

	public NativeArray<TemperatureSpatialEntry> spatialBuffer;
	
	public NativeArray<int> spatialOffsetsBuffer;

	public NativeArray<TemperatureState> temperatureStateBuffer;

	public EntityQuery temperatureQuery;

	public JobHandle handle;


	private int bufferCapacity;

	private ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexTypeHandle;

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
		temperatureIndexTypeHandle = state.GetComponentTypeHandle<TemperaturePartionedIndex>(isReadOnly: false);
		spatialIndexTypeHandle = state.GetComponentTypeHandle<SpatiallyPartionedIndex>(isReadOnly: true);
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
		spatialIndexTypeHandle.Update(ref state);
		
		var writeTemperaturePartionedHandle = PartitionedIndexJobUtility.ScheduleWritePartitionedIndex(temperatureQuery, temperatureIndexTypeHandle, state.Dependency);
		state.Dependency = writeTemperaturePartionedHandle;
		
		var calculateBaseEntityIndexHandle = temperatureQuery.CalculateBaseEntityIndexArrayAsync(Allocator.TempJob, writeTemperaturePartionedHandle, out var indexHandle);
		var buildSpatialEntriesJob = new BuildSpatialEntriesWithIndexJob<TemperatureSpatialEntry, TemperaturePartionedIndex>()
		{
			entityIndexes = calculateBaseEntityIndexHandle,
			spatialBuffer = spatialBuffer,
			spatialOffsetsBuffer = spatialOffsetsBuffer,
			predictedPositionsBuffer = spatialPartioningCoordinator.predictedPositionsBuffer,
			radius = simulationConfig.radius,
			hashingLimit = spatialPartioningCoordinator.hashingLimit,
			spatialIndexHandle = spatialIndexTypeHandle,
			componentIndexHandle = temperatureIndexTypeHandle,
		};
		handle = buildSpatialEntriesJob.ScheduleParallel(temperatureQuery, indexHandle);
	}
}