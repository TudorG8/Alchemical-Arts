using PotionCraft.Core.Authoring;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Systems
{

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(TransformSystemGroup))]
	partial struct LiquidSpawningSystem : ISystem
	{
		private EntityQuery spawnerCountQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<_WrigglerData>();
			state.RequireForUpdate<_FolderManagerData>();
			spawnerCountQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<_LiquidSpawner>()
				.Build(ref state);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var elapsedTime = SystemAPI.Time.ElapsedTime;
			var commandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
			var liquidFolder = SystemAPI.GetSingleton<_FolderManagerData>().LiquidFolder;

			var wriggler = SystemAPI.GetSingleton<_WrigglerData>();
			var limit = wriggler.SpawnLimit;
			int count = spawnerCountQuery.CalculateEntityCount();
			var actualLimit = limit / count;
			foreach(var (localToWorld, liquidSpawner) in SystemAPI.Query<RefRW<LocalToWorld>, RefRW<_LiquidSpawner>>())
			{
				if (liquidSpawner.ValueRO.Timer > elapsedTime) continue;
				if (liquidSpawner.ValueRO.Count >= actualLimit) continue;

				var obj = commandBuffer.Instantiate(liquidSpawner.ValueRO.Liquid);
				commandBuffer.SetComponent(obj, LocalTransform.FromPosition(localToWorld.ValueRO.Position));
				commandBuffer.AddComponent(obj, new Parent() { Value = liquidFolder });
				liquidSpawner.ValueRW.Count++;

				liquidSpawner.ValueRW.Timer = elapsedTime + liquidSpawner.ValueRO.Cooldown;
			}
		}
	}
}