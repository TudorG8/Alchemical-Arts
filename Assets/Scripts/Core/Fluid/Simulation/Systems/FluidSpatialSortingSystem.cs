using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(SortSpatialEntriesJob<FluidSpatialEntry, FluidSpatialEntryComparer>))]
[assembly: RegisterGenericJobType(typeof(BuildSpatialOffsetsJob<FluidSpatialEntry>))]

[UpdateInGroup(typeof(SpatialSortingGroup))]
public partial struct FluidSpatialSortingSystem : ISystem
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
		ref var fluidCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidCoordinatorSystem>();
		if (fluidCoordinatorSystem.fluidCount == 0)
			return;


		var sortSpatialEntriesJob = new SortSpatialEntriesJob<FluidSpatialEntry, FluidSpatialEntryComparer>()
		{
			spatial = fluidCoordinatorSystem.spatialBuffer,
			spatialComparer = new FluidSpatialEntryComparer(),
			count = spatialCoordinatorSystem.count,
		};
		var sortSpatialEntriesHandle = sortSpatialEntriesJob.Schedule(fluidCoordinatorSystem.handle);

		var buildSpatialKeyOffsetsJob = new BuildSpatialOffsetsJob<FluidSpatialEntry>()
		{
			spatial = fluidCoordinatorSystem.spatialBuffer,
			spatialOffsets = fluidCoordinatorSystem.spatialOffsetsBuffer
		};
		handle = buildSpatialKeyOffsetsJob.Schedule(fluidCoordinatorSystem.fluidCount, 64, sortSpatialEntriesHandle);
		state.Dependency = handle;
	}
}