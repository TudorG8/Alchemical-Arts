using PotionCraft.Core.Naming.Components;
using PotionCraft.Gameplay.Prototype.Components;
using PotionCraft.Gameplay.Prototype.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Prototype.Systems
{
	[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
	[UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
	public partial struct FluidSpawningSystem : ISystem
	{
		private EntityQuery validFluidSpawnerQuery;

		private Random random;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			validFluidSpawnerQuery = SystemAPI.QueryBuilder()
				.WithAll<LocalToWorld>()
				.WithAll<FluidSpawnerState>()
				.WithAll<FluidSpawnerConfig>()
				.Build();
			random = new Random(0x6E624EB7u);
			
			state.RequireForUpdate<FolderManagerConfig>();
			state.RequireForUpdate(validFluidSpawnerQuery);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var elapsedTime = SystemAPI.Time.ElapsedTime;
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
			var fluidFolder = SystemAPI.GetSingleton<FolderManagerConfig>().fluidFolder;

			var spawnFluidEntitiesJob = new SpawnFluidEntitiesJob
			{
				ecb = commandBuffer,
				elapsedTime = elapsedTime,
				baseSeed = random.NextUInt(),
				folder = fluidFolder
			};
			state.Dependency = spawnFluidEntitiesJob.ScheduleParallel(validFluidSpawnerQuery, state.Dependency);
		}
	}
}