using PotionCraft.Core.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace PotionCraft.Core.Systems
{
	partial struct SetEntityNameSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach(var (entityName, entity) in SystemAPI.Query<_EntityNameData>().WithEntityAccess())
			{
				state.EntityManager.SetName(entity, entityName.Value);
				state.EntityManager.SetComponentEnabled<_EntityNameData>(entity, false);
			}
		}
	}
}
