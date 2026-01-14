using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(SpatialSortingGroup))]
	public partial struct SpatialSortingSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var spatialPartioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartioningSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			spatialPartioningSystem.handle.Complete();
			state.Dependency = handle = spatialCoordinatorSystem.spatialBuffer.Slice(0, spatialCoordinatorSystem.count).SortJob(new SpatialEntryKeyComparer()).Schedule();
		}
	}
}