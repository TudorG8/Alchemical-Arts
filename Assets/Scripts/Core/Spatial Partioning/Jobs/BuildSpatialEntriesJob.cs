using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Models;
using PotionCraft.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct BuildSpatialEntriesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<SpatialEntry> spatial;

		[WriteOnly]
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
			[EntityIndexInQuery] int index)
		{
			var cell = SpatialHashingUtility.GetCell2D(predictedPositions[index], radius);
			var hash = SpatialHashingUtility.HashCell2D(cell);
			var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
			
			spatial[index] = new SpatialEntry() { index = index, key = cellKey };
			spatialOffsets[index] = int.MaxValue;
		}
	}
}