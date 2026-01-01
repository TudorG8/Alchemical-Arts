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
	[UpdateAfter(typeof(FluidPositionInitializationSystem))]
	partial struct PositionPredictionSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<float2> predictedPositionsBuffer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationState>();
			predictedPositionsBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			predictedPositionsBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			if (fluidPositionInitializationSystem.count == 0)
				return;

			var simulationState = SystemAPI.GetSingleton<SimulationState>();

			var predictPositionsJob = new PredictPositionsJob
			{
				positions = fluidPositionInitializationSystem.positionBuffer,
				velocities = fluidPositionInitializationSystem.velocityBuffer,
				predictedPositions = predictedPositionsBuffer,
				predictionFactor = 1f / simulationState.predictionFrames,
			};
			handle = predictPositionsJob.ScheduleParallel(fluidPositionInitializationSystem.handle);
		}
	}
}