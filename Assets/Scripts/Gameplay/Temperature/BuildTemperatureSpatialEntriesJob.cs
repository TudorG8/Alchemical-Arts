using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


[BurstCompile]
public partial struct BuildTemperatureSpatialEntriesJob : IJobEntity
{
	[NativeDisableParallelForRestriction]
	public NativeArray<TemperatureSpatialEntry> fluidSpatialBuffer;

	[NativeDisableParallelForRestriction]
	public NativeArray<int> fluidSpatialOffsetsBuffer;

	[ReadOnly]
	public NativeArray<float2> predictedPositionsBuffer;
	
	[ReadOnly]
	public float radius;

	[ReadOnly]
	public int count;

	[ReadOnly]
	public int hashingLimit;


	public void Execute(
		in SpatiallyPartionedIndex spatiallyPartionedItemState,
		in TemperaturePartionedIndex temperaturePartionedIndex)
	{
		var cell = SpatialHashingUtility.GetCell2D(predictedPositionsBuffer[spatiallyPartionedItemState.index], radius);
		var hash = SpatialHashingUtility.HashCell2D(cell);
		var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
		
		fluidSpatialBuffer[temperaturePartionedIndex.Index] = new TemperatureSpatialEntry() { simulationIndex = spatiallyPartionedItemState.index, temperatureIndex = temperaturePartionedIndex.Index, key = cellKey };
		fluidSpatialOffsetsBuffer[temperaturePartionedIndex.Index] = int.MaxValue;
	}
}
