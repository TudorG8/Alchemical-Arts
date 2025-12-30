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


		private NativeArray<int2> offsets2D;

		private float spikyPow3ScalingFactor;

		private float spikyPow2ScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();

			densities = new NativeArray<float>(10000, Allocator.Persistent);
			nearDensity = new NativeArray<float>(10000, Allocator.Persistent);

			offsets2D = new NativeArray<int2>(9, Allocator.Persistent);
			offsets2D[0] = new int2(-1, 1);
			offsets2D[1] = new int2(0, 1);
			offsets2D[2] = new int2(1, 1);
			offsets2D[3] = new int2(-1, 0);
			offsets2D[4] = new int2(0, 0);
			offsets2D[5] = new int2(1, 0);
			offsets2D[6] = new int2(-1, -1);
			offsets2D[7] = new int2(0, -1);
			offsets2D[8] = new int2(1, -1);

			spikyPow3ScalingFactor = 10 / (math.PI * math.pow(0.35f, 5));
			spikyPow2ScalingFactor = 6 / (math.PI * math.pow(0.35f, 4));
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			densities.Dispose();
			nearDensity.Dispose();
			offsets2D.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var computeDensitiesJob = new ComputeDensitiesJob()
			{
				smoothingRadius = simulationConfig.radius,
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				spatial = spatialPartitioningSystem.Spatial,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				numParticles = fluidPositionInitializationSystem.count,
				offsets2D = offsets2D,
				spikyPow2ScalingFactor = spikyPow2ScalingFactor,
				spikyPow3ScalingFactor = spikyPow3ScalingFactor,
				densities = densities,
				nearDensities = nearDensity,
				hashingLimit = 10000
			};
			handle = computeDensitiesJob.ScheduleParallel(spatialPartitioningSystem.handle);
		}
	}
}