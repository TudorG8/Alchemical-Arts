using PotionCraft.Core.Naming.Authoring;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Systems
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
	partial struct LiquidSpawningSystem : ISystem
	{
		private EntityQuery validLiquidSpawnerQuery;

		private Random random;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			validLiquidSpawnerQuery = SystemAPI.QueryBuilder()
				.WithAll<LocalToWorld>()
				.WithAll<LiquidSpawner>()
				.Build();
			random = new Random(0x6E624EB7u);
			
			state.RequireForUpdate<FolderManagerData>();
			state.RequireForUpdate(validLiquidSpawnerQuery);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var elapsedTime = SystemAPI.Time.ElapsedTime;
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
			var liquidFolder = SystemAPI.GetSingleton<FolderManagerData>().LiquidFolder;

			var spawnLiquidJob = new SpawnLiquidJob
			{
				ecb = commandBuffer,
				elapsedTime = elapsedTime,
				baseSeed = random.NextUInt(),
				folder = liquidFolder
			};
			state.Dependency = spawnLiquidJob.ScheduleParallel(validLiquidSpawnerQuery, state.Dependency);
		}

		[BurstCompile]
		public partial struct SpawnLiquidJob : IJobEntity
		{
			[WriteOnly]
			public EntityCommandBuffer.ParallelWriter ecb;

			[ReadOnly]
			public double elapsedTime;

			[ReadOnly]
			public uint baseSeed;

			[ReadOnly]
			public Entity folder;


			void Execute(
				[EntityIndexInQuery] int index,
				ref LiquidSpawner liquidSpawner,
				in LocalToWorld localToWorld)
			{
				if (liquidSpawner.timer == 0) 
				{
					liquidSpawner.timer = elapsedTime + liquidSpawner.delay; return;
				}
				
				if (liquidSpawner.timer > elapsedTime) return;


				if (liquidSpawner.count >= liquidSpawner.max) return; 

				var random = new Random(baseSeed + (uint)index);

				var obj = ecb.Instantiate(index, liquidSpawner.liquid);
				ecb.SetComponent(index, obj, LocalTransform.FromPosition(localToWorld.Position + new float3(random.NextFloat(-0.1f, 0.1f), random.NextFloat(-0.1f, 0.1f), 0)));
				ecb.AddComponent(index, obj, new Parent() { Value = folder });
				ecb.AddComponent(index, obj, new PreviousParent() { Value = folder });
				
				liquidSpawner.count++;
				liquidSpawner.timer = elapsedTime + liquidSpawner.delay;
			}
		}
	}
}