using PotionCraft.Core.Physics.Components;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LiquidPhysicsGroup))]
[UpdateAfter(typeof(PopulateLiquidPositionsSystem))]
partial struct ApplyGravitySystem : ISystem
{
	private SystemHandle populateLiquidPositionsSystemHandle;

	private float gravity;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldConfigComponent>();
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
public partial struct ApplyGravityJob : IJobEntity
{
	public NativeArray<float2> velocities;
	
	[ReadOnly]
	public float deltaTime;

	[ReadOnly]
	public float gravity;


	void Execute(
		[EntityIndexInQuery] int index,
		in _LiquidTag _)
	{
		velocities[index] += new float2(0, gravity) * deltaTime;
	}
}