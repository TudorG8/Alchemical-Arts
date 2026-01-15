using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct WriteIndexedNativeArray<T, U> : IJobChunk
		where T : unmanaged, IComponentData, IIndexedComponent
		where U : unmanaged, IComponentData
	{
		public ComponentTypeHandle<U> componentHandle;
		
		[NativeDisableParallelForRestriction]
		public NativeArray<U> buffer;

		[ReadOnly]
		public ComponentTypeHandle<T> spatialIndexHandle;


		public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
		{
			var spatialIndexes = chunk.GetNativeArray(ref spatialIndexHandle);
			var components = chunk.GetNativeArray(ref componentHandle);
			
			for(int i = 0; i < chunk.Count; i++)
			{
				components[i] = buffer[spatialIndexes[i].Index];
			}
		}
	}
}