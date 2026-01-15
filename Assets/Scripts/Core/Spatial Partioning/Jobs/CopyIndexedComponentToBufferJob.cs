using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct CopyIndexedComponentToBufferJob<T, U> : IJobChunk 
		where T : unmanaged, IComponentData
		where U : unmanaged, IComponentData, IIndexedComponent
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<T> componentBuffer;
		
		[ReadOnly]
		public ComponentTypeHandle<T> componentTypeHandle;

		[ReadOnly]
		public ComponentTypeHandle<U> indexerHandle;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var components = chunk.GetNativeArray(ref componentTypeHandle);
			var indexers = chunk.GetNativeArray(ref indexerHandle);

			for(int i = 0; i < chunk.Count; i++)
			{
				componentBuffer[indexers[i].Index] = components[i];
			}
		}
	}
}