using PotionCraft.Core.LiquidSimulation.Components;
using PotionCraft.Core.LiquidSimulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.LiquidSimulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(PopulateLiquidPositionsSystem))]
	partial struct ApplyGravitySystem : ISystem
	{
		private SystemHandle populateLiquidPositionsSystemHandle;

		private float gravity;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			populateLiquidPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();

			gravity = -10f;
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var applyGravityJob = new ApplyGravityJob
			{
				velocities = populateLiquidPositionsSystem.velocityBuffer,
				deltaTime = SystemAPI.Time.DeltaTime,
				gravity = gravity
			};
			var applyHandle = applyGravityJob.ScheduleParallel(state.Dependency);
			applyHandle.Complete();
		}
	}

	[BurstCompile]
	[WithAll(typeof(LiquidTag))]
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
			velocities[index] += new float2(0, gravity) * deltaTime;
		}
	}
}