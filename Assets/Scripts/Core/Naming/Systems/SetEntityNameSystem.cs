using PotionCraft.Core.Naming.Authoring;
using PotionCraft.Core.Naming.Groups;
using Unity.Burst;
using Unity.Entities;

namespace PotionCraft.Core.Naming.Systems
{
	[UpdateInGroup(typeof(NamingInitializationGroup), OrderFirst = true)]
	partial struct SetEntityNameSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach(var (entityName, entity) in SystemAPI.Query<_EntityNameData>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab))
			{
				state.EntityManager.SetName(entity, entityName.Value);
				state.EntityManager.SetComponentEnabled<_EntityNameData>(entity, false);
			}
		}
	}
}
