using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
partial struct LiquidSpawningSystem : ISystem
{
	private EntityQuery spawnerCountQuery;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<Wriggler>();
		spawnerCountQuery = new EntityQueryBuilder(Allocator.Temp)
			.WithAll<LiquidSpawner>()
			.Build(ref state);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var elapsedTime = SystemAPI.Time.ElapsedTime;
		var commandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		
		var wriggler = SystemAPI.GetSingleton<Wriggler>();
		var limit = wriggler.limit;
		int count = spawnerCountQuery.CalculateEntityCount();
		var actualLimit = limit / count;
		foreach(var (localToWorld, liquidSpawner) in SystemAPI.Query<RefRW<LocalToWorld>, RefRW<LiquidSpawner>>())
		{
			if (liquidSpawner.ValueRO.timer > elapsedTime) continue;
			if (liquidSpawner.ValueRO.count >= actualLimit) continue;

			var obj = commandBuffer.Instantiate(liquidSpawner.ValueRO.liquid);
			commandBuffer.SetComponent(obj, LocalTransform.FromPosition(localToWorld.ValueRO.Position));
			liquidSpawner.ValueRW.count++;

			liquidSpawner.ValueRW.timer = elapsedTime + liquidSpawner.ValueRO.cooldown;
		}
	}
}
