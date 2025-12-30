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
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(LiquidInwardsInputSystem))]
	partial struct PositionPredictionSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<float2> predictedPositionsBuffer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();
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
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<LiquidPositionInitializationSystem>();
			ref var inputLiquidForcesSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<LiquidInwardsInputSystem>();

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var predictPositionsJob = new PredictPositionsJob
			{
				positions = populateLiquidPositionsSystem.positionBuffer,
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				predictedPositions = predictedPositionsBuffer,
				predictionFactor = 1f / simulationConfig.predictionFrames,
			};
			handle = predictPositionsJob.ScheduleParallel(inputLiquidForcesSystem.handle);
		}
	}
}