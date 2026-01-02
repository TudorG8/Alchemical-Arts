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
	partial struct FluidSpawnSystem : ISystem
	{
		private EntityQuery validFluidSpawnerQuery;

		private Random random;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			validFluidSpawnerQuery = SystemAPI.QueryBuilder()
				.WithAll<LocalToWorld>()
				.WithAll<FluidSpawner>()
				.Build();
			random = new Random(0x6E624EB7u);
			
			state.RequireForUpdate<FolderManagerData>();
			state.RequireForUpdate(validFluidSpawnerQuery);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var elapsedTime = SystemAPI.Time.ElapsedTime;
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
			var fluidFolder = SystemAPI.GetSingleton<FolderManagerData>().FluidFolder;

			var spawnFluidEntitiesJob = new SpawnFluidEntitiesJob
			{
				ecb = commandBuffer,
				elapsedTime = elapsedTime,
				baseSeed = random.NextUInt(),
				folder = fluidFolder
			};
			state.Dependency = spawnFluidEntitiesJob.ScheduleParallel(validFluidSpawnerQuery, state.Dependency);
		}

		[BurstCompile]
		public partial struct SpawnFluidEntitiesJob : IJobEntity
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
				ref FluidSpawner fluidSpawner,
				in LocalToWorld localToWorld)
			{
				if (fluidSpawner.timer == 0) 
				{
					fluidSpawner.timer = elapsedTime + fluidSpawner.delay; return;
				}
				
				if (fluidSpawner.timer > elapsedTime) return;


				if (fluidSpawner.count >= fluidSpawner.max) return; 

				var random = new Random(baseSeed + (uint)index);

				var obj = ecb.Instantiate(index, fluidSpawner.fluid);
				ecb.SetComponent(index, obj, LocalTransform.FromPosition(localToWorld.Position + new float3(random.NextFloat(-0.1f, 0.1f), random.NextFloat(-0.1f, 0.1f), 0)));
				ecb.AddComponent(index, obj, new Parent() { Value = folder });
				ecb.AddComponent(index, obj, new PreviousParent() { Value = folder });
				
				fluidSpawner.count++;
				fluidSpawner.timer = elapsedTime + fluidSpawner.delay;
			}
		}
	}
}