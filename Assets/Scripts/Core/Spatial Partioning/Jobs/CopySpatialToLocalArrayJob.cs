
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct CopySpatialToLocalArrayJob<T, U> : IJobChunk 
		where T : unmanaged, IComponentData, IIndexedComponent
		where U : unmanaged
	{
		[ReadOnly]
		public NativeArray<U> positionsBufferInput;

		[NativeDisableParallelForRestriction]
		public NativeArray<U> positionsBufferOutput;

		[ReadOnly]
		public ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexersHandle;

		[ReadOnly]
		public ComponentTypeHandle<T> indexerHandle;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var spatialIndexers = chunk.GetNativeArray(ref spatialIndexersHandle);
			var indexers = chunk.GetNativeArray(ref indexerHandle);

			for(int i = 0; i < chunk.Count; i++)
			{
				positionsBufferOutput[indexers[i].Index] = positionsBufferInput[spatialIndexers[i].Index];
			}
		}
	}
}