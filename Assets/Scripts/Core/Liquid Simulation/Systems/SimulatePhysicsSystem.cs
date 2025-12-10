using PotionCraft.Core.Physics.Components;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(LiquidPhysicsGroup))]
[UpdateAfter(typeof(WriteLiquidVelocitiesSystem))]
partial struct SimulatePhysicsSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldConfigComponent>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var worldQuerry = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldConfigComponent>().Build(state.EntityManager);
		var world = worldQuerry.GetSingletonEntity();
		var worldScript = state.EntityManager.GetComponentData<PhysicsWorldConfigComponent>(world);
		worldScript.physicsWorld.Simulate(SystemAPI.Time.DeltaTime);
	}
}