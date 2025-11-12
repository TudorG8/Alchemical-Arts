using PotionCraft.Core.Naming.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.SearchService;
using UnityEngine;

namespace PotionCraft.Core.Naming.Systems
{
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
