#if UNITY_EDITOR
using PotionCraft.Core.Naming.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.Systems
{
	partial struct SetDebugHelperNameSystem : ISystem
	{
		public const string NAME_REFERENCE = "PublicEntityRef";

		private EntityQuery entitiesWithoutNameQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			entitiesWithoutNameQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<SceneSection, SceneTag>()
				.WithAbsent<_EntityNameData>()
				.Build(ref state);
			state.RequireForUpdate(entitiesWithoutNameQuery);
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
					if (type.GetManagedType().Name == NAME_REFERENCE)
					{
						commandBuffer.AddComponent(entity, new _EntityNameData() { Value = "Unity Debug Helper"});
						continue;
					}
					commandBuffer.AddComponent(entity, new _EntityNameData() { Value = state.EntityManager.GetName(entity)});
				}
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}
#endif