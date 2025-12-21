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
	[UpdateAfter(typeof(PopulateLiquidPositionsSystem))]
	partial struct ApplyGravitySystem : ISystem
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
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PopulateLiquidPositionsSystem>();
			if (populateLiquidPositionsSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SimulationConfig>();

			var applyGravityJob = new ApplyGravityJob
			{
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = simulationConfig.gravity
			};
			handle = applyGravityJob.ScheduleParallel(populateLiquidPositionsSystem.handle);
		}
	}

	[BurstCompile]
	[WithAll(typeof(LiquidTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyGravityJob : IJobEntity
	{
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float gravity;


		void Execute(
			[EntityIndexInQuery] int index)
		{
			velocities[index] -= new float2(0, gravity) * deltaTime;
		}
	}
}