using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct BuildSpatialOffsetsJob<T> : IJobParallelFor where T : unmanaged, ISpatialEntry
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsets;
		
		[ReadOnly]
		public NativeArray<T> spatial;


		public void Execute(int index)
		{
			var key = spatial[index].Key;
			var prevKey = index == 0 ? int.MaxValue : spatial[index - 1].Key;
			if (key != prevKey)
			{
				spatialOffsets[key] = index;
			}
		}
	}
}