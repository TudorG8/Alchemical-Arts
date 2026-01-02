using PotionCraft.Core.Naming.Components;
using PotionCraft.Core.Naming.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.Systems
{
	[UpdateInGroup(typeof(NamingInitializationGroup))]
	partial struct EntityNamingSystem : ISystem
	{
		private EntityQuery entitiesWithNameDataQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			entitiesWithNameDataQuery = SystemAPI.QueryBuilder().WithAll<EntityNameConfig>().WithOptions(EntityQueryOptions.IncludePrefab).Build();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			if (entitiesWithNameDataQuery.CalculateEntityCount() == 0)
				return;

			var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
			var commandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

			var writeParticlesJob = new SetNameJob
			{
				ecb = commandBuffer,
			};
			state.Dependency = writeParticlesJob.ScheduleParallel(entitiesWithNameDataQuery, state.Dependency);
		}
	}

	[BurstCompile]
	public partial struct SetNameJob : IJobEntity
	{
		[WriteOnly]
		public EntityCommandBuffer.ParallelWriter ecb;

		void Execute(
			[EntityIndexInQuery] int index,
			Entity entity,
			ref EntityNameConfig name)
		{
			ecb.SetName(index, entity, name.Value);
			ecb.SetComponentEnabled<EntityNameConfig>(index, entity, false);
		}
	}
}
