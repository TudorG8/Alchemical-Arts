using AlchemicalArts.Core.Physics.Components;
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
	[WithAll(typeof(SpatiallyPartionedItemState))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct BuildSpatialEntriesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<SpatialEntry> spatial;

		[WriteOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<float2> predictedPositionsBuffer;
		
		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int count;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index,
			in SpatiallyPartionedItemState spatiallyPartionedItemState,
			in Entity entity)
		{
			var cell = SpatialHashingUtility.GetCell2D(predictedPositionsBuffer[spatiallyPartionedItemState.index], radius);
			var hash = SpatialHashingUtility.HashCell2D(cell);
			var cellKey = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
			
			spatial[spatiallyPartionedItemState.index] = new SpatialEntry() { simulationIndex = spatiallyPartionedItemState.index, key = cellKey, entity = entity };
			spatialOffsets[spatiallyPartionedItemState.index] = int.MaxValue;
		}
	}
}