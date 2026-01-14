using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Burst.Intrinsics;
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

	[BurstCompile]
	public partial struct BuildSpatialEntriesJob<T, U> : IJobChunk 
		where T : unmanaged, ISpatialEntryIndexer
		where U : unmanaged, IComponentData, IIndexedComponent
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<T> spatialBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsetsBuffer;

		[ReadOnly]
		public NativeArray<float2> predictedPositionsBuffer;

		[ReadOnly]
		public ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexHandle;
		
		public ComponentTypeHandle<U> componentIndexHandle;

		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int hashingLimit;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<int> baseEntityIndex;
		
		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var spatialComponents = chunk.GetNativeArray(ref spatialIndexHandle);
			var genericComponents = chunk.GetNativeArray(ref componentIndexHandle);
			var baseIndex = baseEntityIndex[unfilteredChunkIndex];
			
			for(int i = 0; i < chunk.Count; i++)
			{
				var index = baseIndex + i;
				
				var spatialElement = spatialBuffer[index];
				var cellKey = SpatialHashingUtility.GetCellKey(predictedPositionsBuffer[spatialComponents[i].Index], radius, hashingLimit);
				spatialElement.SimulationIndex = spatialComponents[i].Index;
				spatialElement.Index = genericComponents[i].Index;
				spatialElement.Key = cellKey;
				
				spatialBuffer[index] = spatialElement;
			}
		}
	}
}