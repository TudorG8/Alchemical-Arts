using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct WriteSpatiallyPartionedIndexJob : IJobEntity
	{
		public void Execute(
			[EntityIndexInQuery] int index,
			ref SpatiallyPartionedIndex spatiallyPartionedIndex)
		{
			spatiallyPartionedIndex.index = index;
		}
	}
}