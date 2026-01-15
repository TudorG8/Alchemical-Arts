using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct BuildSpatialEntriesWithIndexJob<T, U> : IJobChunk 
		where T : unmanaged, ISpatialEntryIndexer
		where U : unmanaged, IComponentData, IIndexedComponent
	{
		public ComponentTypeHandle<U> componentIndexHandle;

		[NativeDisableParallelForRestriction]
		public NativeArray<T> spatialBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> spatialOffsetsBuffer;

		[ReadOnly]
		public NativeArray<float2> predictedPositionsBuffer;

		[ReadOnly]
		public ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexHandle;
		
		[ReadOnly]
		public float radius;

		[ReadOnly]
		public int hashingLimit;

		[ReadOnly]
		public NativeArray<int> entityIndexes;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var spatialComponents = chunk.GetNativeArray(ref spatialIndexHandle);
			var genericComponents = chunk.GetNativeArray(ref componentIndexHandle);
			var baseIndex = entityIndexes[unfilteredChunkIndex];
			
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