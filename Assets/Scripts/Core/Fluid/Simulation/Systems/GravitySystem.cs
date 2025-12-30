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
	[UpdateAfter(typeof(LiquidPositionInitializationSystem))]
	partial struct GravitySystem : ISystem
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
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<LiquidPositionInitializationSystem>();
			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var applyGravityForcesJob = new ApplyGravityForcesJob
			{
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = simulationConfig.gravity
			};
			handle = applyGravityForcesJob.ScheduleParallel(populateLiquidPositionsSystem.handle);
		}
	}
}