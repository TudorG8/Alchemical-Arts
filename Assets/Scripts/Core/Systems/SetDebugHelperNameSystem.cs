using PotionCraft.Core.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Systems
{
	partial struct SetDebugHelperNameSystem : ISystem
	{
		private EntityQuery entitiesWithoutNameQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			entitiesWithoutNameQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<SceneSection, SceneTag>()
				.WithAbsent<_EntityNameData>()
				.Build(ref state);
		}

		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
				
			var noNameEntities = entitiesWithoutNameQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in noNameEntities)
			{
				var componentTypes = state.EntityManager.GetComponentTypes(entity, Allocator.Temp);
				foreach(var type in componentTypes)
				{
					if (type.GetManagedType().Name == "PublicEntityRef")
					{
						commandBuffer.AddComponent(entity, new _EntityNameData() { Value = "Unity Debug Helper"});
					}
				}
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}