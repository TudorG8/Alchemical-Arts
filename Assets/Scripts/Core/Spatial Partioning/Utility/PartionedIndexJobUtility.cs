using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Utility
{
	public static class PartitionedIndexJobUtility
	{
		public static JobHandle ScheduleWritePartitionedIndex<T>(
			EntityQuery query,
			ComponentTypeHandle<T> componentTypeHandle,
			JobHandle dependencies = default)
				where T : unmanaged, IComponentData, IIndexedComponent
		{
			var calculateBaseEntityIndexHandle = query.CalculateBaseEntityIndexArrayAsync(Allocator.TempJob, dependencies, out var indexHandle);

			var writePartionedIndexJob = new WritePartionedIndexJob<T>
			{
				componentTypeHandle = componentTypeHandle,
				entityIndexes = calculateBaseEntityIndexHandle
			};
			return writePartionedIndexJob.ScheduleParallel(query, indexHandle);
		}
	}
}