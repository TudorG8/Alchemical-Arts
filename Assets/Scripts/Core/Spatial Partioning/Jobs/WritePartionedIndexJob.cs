using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct WritePartionedIndexJob<T> : IJobChunk where T : unmanaged, IComponentData, IIndexedComponent
	{
		public ComponentTypeHandle<T> componentTypeHandle;

		[ReadOnly]
		[DeallocateOnJobCompletion]
		public NativeArray<int> entityIndexes;
		
		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var components = chunk.GetNativeArray(ref componentTypeHandle);
			var baseIndex = entityIndexes[unfilteredChunkIndex];
			
			for(int i = 0; i < chunk.Count; i++)
			{
				var component = components[i];
				component.Index = baseIndex + i;
				components[i] = component;
			}
		}
	}
}