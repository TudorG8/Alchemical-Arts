using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Fluid.Simulation.Utility;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSort))]
[assembly: RegisterGenericJobType(typeof(SortJob<SpatialEntry, SpatialEntryKeyComparer>.SegmentSortMerge))]

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(PositionPredictionSystem))]
	partial struct SpatialPartitioningSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<SpatialEntry> Spatial;
		
		public NativeArray<int> SpatialOffsets;


		private SpatialEntryKeyComparer spatialEntryKeyComparer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();
			Spatial = new NativeArray<SpatialEntry>(10000, Allocator.Persistent);
			SpatialOffsets = new NativeArray<int>(10000, Allocator.Persistent);
			spatialEntryKeyComparer = new SpatialEntryKeyComparer();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			Spatial.Dispose();
			SpatialOffsets.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var buildSpatialEntriesJob = new BuildSpatialEntriesJob
			{
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				radius = simulationConfig.radius,
				count = fluidPositionInitializationSystem.count,
				spatialOutput = Spatial,
				spatialOffsetOutput = SpatialOffsets,
				hashingLimit = 10000
			};
			var buildSpatialEntriesHandle = buildSpatialEntriesJob.ScheduleParallel(positionPredictionSystem.handle);
			buildSpatialEntriesHandle.Complete(); // SortJob won't work without completing this

			var sortJobHandle = Spatial.Slice(0, fluidPositionInitializationSystem.count).SortJob(spatialEntryKeyComparer).Schedule();

			var buildSpatialKeyOffsetsJob = new BuildSpatialKeyOffsetsJob()
			{
				spatial = Spatial,
				spatialOffsets = SpatialOffsets
			};
			handle = buildSpatialKeyOffsetsJob.ScheduleParallel(sortJobHandle);
			handle.Complete(); // Completing before we start doing actual work with forces
		}
	}
}