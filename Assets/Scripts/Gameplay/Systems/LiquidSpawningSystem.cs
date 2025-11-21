using PotionCraft.Core.Naming.Authoring;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Systems
{

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(TransformSystemGroup))]
	partial struct LiquidSpawningSystem : ISystem
	{
		private EntityQuery spawnerCountQuery;

		private Random random;


		// [BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<_WrigglerData>();
			state.RequireForUpdate<_FolderManagerData>();
			spawnerCountQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<_LiquidSpawner>()
				.Build(ref state);
			random = new Random(0x6E624EB7u);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var elapsedTime = SystemAPI.Time.ElapsedTime;
			var commandBuffer = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
			var liquidFolder = SystemAPI.GetSingleton<_FolderManagerData>().LiquidFolder;

			var wriggler = SystemAPI.GetSingleton<_WrigglerData>();
			foreach(var (localToWorld, liquidSpawner) in SystemAPI.Query<RefRW<LocalToWorld>, RefRW<_LiquidSpawner>>())
			{
				if (liquidSpawner.ValueRO.Timer > elapsedTime) continue;
				if (liquidSpawner.ValueRO.Count >= wriggler.LimitPerSpawner) continue;

				var obj = commandBuffer.Instantiate(wriggler.Liquid);
				commandBuffer.SetComponent(obj, LocalTransform.FromPosition(localToWorld.ValueRO.Position + new float3(random.NextFloat(-0.01f, 0.01f), 0, 0)));
				commandBuffer.AddComponent(obj, new Parent() { Value = liquidFolder });
				liquidSpawner.ValueRW.Count++;

				liquidSpawner.ValueRW.Timer = elapsedTime + wriggler.SpawnerDelay;
			}
		}
	}
}