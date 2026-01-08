using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{		
	[BurstCompile]
	public partial struct BuildSpatialKeyOffsetsJob : IJobParallelFor
	{
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsets;
		
		[ReadOnly]
		public NativeArray<FluidSpatialEntry> spatial;


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
}