using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(InputLiquidForceSystem))]
	partial struct CalculatePredictedPositionsSystem : ISystem
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
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PopulateLiquidPositionsSystem>();
			ref var inputLiquidForcesSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<InputLiquidForceSystem>();

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var calculatePredictedPositionsJob = new CalculatePredictedPositionsJob
			{
				positions = populateLiquidPositionsSystem.positionBuffer,
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				predictedPositions = predictedPositionsBuffer,
				predictionFactor = 1f / simulationConfig.predictionFrames,
			};
			handle = calculatePredictedPositionsJob.ScheduleParallel(inputLiquidForcesSystem.handle);
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		[WithAll(typeof(PhysicsBodyState))]
		public partial struct CalculatePredictedPositionsJob : IJobEntity
		{
			[WriteOnly]
			public NativeArray<float2> predictedPositions;

			[ReadOnly]
			public NativeArray<float2> positions;

			[ReadOnly]
			public NativeArray<float2> velocities;
			
			[ReadOnly]
			public float predictionFactor;
			

			void Execute(
				[EntityIndexInQuery] int index)
			{
				predictedPositions[index] = positions[index] + velocities[index] * predictionFactor;
			}
		}
	}
}