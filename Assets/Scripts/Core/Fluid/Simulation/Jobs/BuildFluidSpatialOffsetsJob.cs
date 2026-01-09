using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{		
	[BurstCompile]
	public partial struct BuildFluidSpatialOffsetsJob : IJobParallelFor
	{
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