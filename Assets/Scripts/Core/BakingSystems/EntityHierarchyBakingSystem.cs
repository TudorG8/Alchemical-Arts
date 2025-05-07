using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
partial struct EntityHierarchyBakingSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesToReparent = new NativeList<NativeArray<TransformLinkData>>(Allocator.Temp);
		var entitiesWithBuffer = new NativeList<Entity>(Allocator.Temp);

		foreach (var (transformLinks, entity) in SystemAPI.Query<DynamicBuffer<TransformLinkData>>()
			.WithOptions(EntityQueryOptions.IncludePrefab)
			.WithEntityAccess())
		{
			entitiesToReparent.Add(transformLinks.ToNativeArray(Allocator.Temp));
			entitiesWithBuffer.Add(entity);
		}
		
		foreach(var entityToReparent in entitiesToReparent)
		{
			foreach(var transformLink in entityToReparent)
			{
				state.EntityManager.AddComponentData(transformLink.Child, new _EntityName { name = transformLink.Name });
				if (transformLink.Parent != Entity.Null)
				{
					state.EntityManager.AddComponentData(transformLink.Child, new Parent { Value = transformLink.Parent });
					state.EntityManager.SetComponentData(transformLink.Child, LocalTransform.FromPosition(transformLink.LocalPosition));
				}
			}
		}

		foreach(var entityWithBuffer in entitiesWithBuffer)
		{
			state.EntityManager.RemoveComponent<TransformLinkData>(entityWithBuffer);
		}
	}
}
