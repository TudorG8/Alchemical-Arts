using PotionCraft.Core.Fluid.Simulation.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public struct SortSpatialEntriesJob : IJob
	{
		public NativeArray<SpatialEntry> Spatial;

		public int count;

		public readonly void Execute()
		{
			var slice = new NativeSlice<SpatialEntry>(Spatial, 0, count);
			var comparer = new SpatialEntryKeyComparer();
			slice.Sort(comparer);
		}
	}
}