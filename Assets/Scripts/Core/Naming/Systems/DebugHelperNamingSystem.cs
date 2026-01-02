#if UNITY_EDITOR
using PotionCraft.Core.Naming.Components;
using PotionCraft.Core.Naming.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.Systems
{
	[UpdateInGroup(typeof(NamingInitializationGroup))]
	public partial struct DebugHelperNamingSystem : ISystem
	{
		public const string NAME_REFERENCE = "PublicEntityRef";

		private EntityQuery entitiesWithoutNameQuery;

		private FixedString64Bytes stringBuffer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			entitiesWithoutNameQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<SceneSection, SceneTag>()
				.WithAbsent<EntityNameConfig>()
				.Build(ref state);
			state.RequireForUpdate(entitiesWithoutNameQuery);
		}

		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
				
			var noNameEntities = entitiesWithoutNameQuery.ToEntityArray(Allocator.Temp);
			foreach (var entity in noNameEntities)
			{
				stringBuffer.Clear();
				var componentTypes = state.EntityManager.GetComponentTypes(entity, Allocator.Temp);
				foreach(var type in componentTypes)
				{
					if (type.GetManagedType().Name == NAME_REFERENCE)
					{
						stringBuffer.Append("Unity Debug Helper");
						break;
					}
				}

				if (stringBuffer.Length == 0)
				{
					stringBuffer.Append($"MISSING NAME: Entity({entity.Version}:{entity.Index})");
				}

				commandBuffer.AddComponent(entity, new EntityNameConfig() { Value = stringBuffer });
			}
		}
	}
}
#endif