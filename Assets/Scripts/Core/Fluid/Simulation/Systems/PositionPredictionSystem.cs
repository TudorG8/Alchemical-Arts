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
	public partial struct PositionPredictionSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SimulationConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidBuffersSystem>();
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidPositionInitializationSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var predictPositionsJob = new PredictPositionsJob
			{
				positions = fluidBuffersSystem.positionBuffer,
				velocities = fluidBuffersSystem.velocityBuffer,
				predictedPositions = fluidBuffersSystem.predictedPositionsBuffer,
				predictionFactor = 1f / simulationConfig.predictionFrames,
			};
			handle = predictPositionsJob.ScheduleParallel(fluidPositionInitializationSystem.handle);
		}
	}
}