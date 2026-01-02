using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{		
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct BuildSpatialKeyOffsetsJob : IJobEntity
	{
		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsets;
		
		[ReadOnly]
		public NativeArray<SpatialEntry> spatial;


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