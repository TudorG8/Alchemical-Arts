using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(SpatialPartitioningSystem))]
	partial struct DensityComputationSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<float> densities;

		public NativeArray<float> nearDensity;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationState>();

			densities = new NativeArray<float>(10000, Allocator.Persistent);
			nearDensity = new NativeArray<float>(10000, Allocator.Persistent);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			densities.Dispose();
			nearDensity.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();
			var simulationConstantsState = SystemAPI.GetSingleton<SimulationConstantsState>();

			var computeDensitiesJob = new ComputeDensitiesJob()
			{
				densities = densities,
				nearDensities = nearDensity,
				spatial = spatialPartitioningSystem.Spatial,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				numParticles = fluidPositionInitializationSystem.count,
				simulationState = simulationState,
				simulationConstantsState = simulationConstantsState,
				hashingLimit = 10000
			};
			handle = computeDensitiesJob.ScheduleParallel(spatialPartitioningSystem.handle);
		}
	}
}