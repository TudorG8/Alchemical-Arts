using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct BuildSpatialEntriesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<SpatialEntry> spatial;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;
		
		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int count;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedIndex,
			in Entity entity)
		{
			var cell = SpatialHashingUtility.GetCell2D(predictedPositions[spatiallyPartionedIndex.index], radius);
			var hash = SpatialHashingUtility.HashCell2D(cell);
			var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
			
			spatial[spatiallyPartionedIndex.index] = new SpatialEntry() { simulationIndex = spatiallyPartionedIndex.index, key = cellKey, entity = entity };
			spatialOffsets[spatiallyPartionedIndex.index] = int.MaxValue;
		}
	}
}