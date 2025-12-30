using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	[UpdateAfter(typeof(PressureForceSystem))]
	partial struct ViscosityForceSystem : ISystem
	{
		public JobHandle handle;


		private NativeArray<int2> offsets2D;

		private float poly6ScalingFactor;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();

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

			poly6ScalingFactor = 4 / (math.PI * math.pow(0.35f, 8));
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			offsets2D.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			ref var positionPredictionSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionPredictionSystem>();
			ref var spatialPartitioningSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialPartitioningSystem>();
			ref var pressureForceSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PressureForceSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var applyViscosityForcesJob = new ApplyViscosityForcesJob()
			{
				predictedPositions = positionPredictionSystem.predictedPositionsBuffer,
				spatialOffsets = spatialPartitioningSystem.SpatialOffsets,
				spatial = spatialPartitioningSystem.Spatial,
				smoothingRadius = simulationConfig.radius,
				offsets2D = offsets2D,
				numParticles = fluidPositionInitializationSystem.count,
				deltaTime = SystemAPI.Time.DeltaTime,
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				poly6ScalingFactor = poly6ScalingFactor,
				viscosityStrength = simulationConfig.viscosityStrength,
				hashingLimit = 10000,
			};
			handle = applyViscosityForcesJob.ScheduleParallel(pressureForceSystem.handle);
		}
	}
}