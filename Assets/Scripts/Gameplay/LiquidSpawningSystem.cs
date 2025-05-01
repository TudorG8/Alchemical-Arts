using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct LiquidSpawningSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var elapsedTime = SystemAPI.Time.ElapsedTime;
		var commandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
		foreach(var (localToWorld, liquidSpawner) in SystemAPI.Query<RefRW<LocalToWorld >, RefRW<LiquidSpawner>>())
		{
			if (liquidSpawner.ValueRO.timer > elapsedTime) continue;
			if (liquidSpawner.ValueRO.count > liquidSpawner.ValueRO.limit) continue;

			var obj = commandBuffer.Instantiate(liquidSpawner.ValueRO.liquid);
			commandBuffer.SetComponent(obj, LocalTransform.FromPosition(localToWorld.ValueRO.Position));
			liquidSpawner.ValueRW.count++;

			liquidSpawner.ValueRW.timer = elapsedTime + liquidSpawner.ValueRO.cooldown;
		}
	}
}
