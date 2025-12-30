using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{		
	[BurstCompile]
	[WithAll(typeof(LiquidTag))]
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