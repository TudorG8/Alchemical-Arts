using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public struct SortSpatialEntriesJob<T, U> : IJob 
		where T : unmanaged 
		where U : unmanaged, IComparer<T>
	{
		public NativeArray<T> spatial;

		public U spatialComparer;

		public int count; 


		public readonly void Execute()
		{
			var slice = new NativeSlice<T>(spatial, 0, count);
			slice.Sort(spatialComparer);
		}
	}
}