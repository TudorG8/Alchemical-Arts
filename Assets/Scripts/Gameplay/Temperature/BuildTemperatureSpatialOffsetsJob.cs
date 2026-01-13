using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[BurstCompile]
public partial struct BuildTemperatureSpatialOffsetsJob : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	public NativeArray<int> spatialOffsets;
	
	[ReadOnly]
	public NativeArray<TemperatureSpatialEntry> spatial;


	public void Execute(
		[EntityIndexInQuery] int index)
	{
		var key = spatial[index].key;
		var prevKey = index == 0 ? int.MaxValue : spatial[index - 1].key;
		if (key != prevKey)
		{
			spatialOffsets[key] = index;
		}
	}
}