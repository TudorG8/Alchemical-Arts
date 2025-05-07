using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct SetEntityNameSystem : ISystem
{
	private EntityQuery entitiesWithoutNameQuery;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		entitiesWithoutNameQuery = new EntityQueryBuilder(Allocator.Temp)
			.WithAbsent<_EntityName>()
			.Build(ref state);
	}


	public void OnUpdate(ref SystemState state)
	{
		var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
	
		foreach(var (entityName, entity) in SystemAPI.Query<_EntityName>().WithEntityAccess())
		{
			state.EntityManager.SetName(entity, entityName.name);
			state.EntityManager.SetComponentEnabled<_EntityName>(entity, false);
		}

		// foreach(var (entityName, entity) in SystemAPI.Query<_EntityName>().WithPresent<Prefab>().WithEntityAccess())
		// {
		// 	state.EntityManager.SetName(entity, $"Prefab - {entityName.name}");
		// 	state.EntityManager.SetComponentEnabled<_EntityName>(entity, false);
		// }

		// var noNameEntities = entitiesWithoutNameQuery.ToEntityArray(Allocator.Temp);
		// foreach (var entity in noNameEntities)
		// {
		// 	var componentTypes = state.EntityManager.GetComponentTypes(entity, Allocator.Temp);
		// 	foreach(var type in componentTypes)
		// 	{
		// 		if (type.GetManagedType().Name == "PublicEntityRef")
		// 		{
		// 			commandBuffer.AddComponent(entity, new _EntityName() { name = "Unity Debug Helper"});
		// 		}
		// 	}
		// }
	}
}
