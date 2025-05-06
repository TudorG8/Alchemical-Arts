using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
partial struct EntityHierarchySystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var entitiesToReparent = new NativeList<(Entity, NeedsReparenting)>(Allocator.Temp);
		foreach (var (needsReparenting, entity) in SystemAPI.Query<NeedsReparenting>().WithEntityAccess())
		{
			entitiesToReparent.Add((entity, needsReparenting));
		}

		foreach (var (entity, target) in entitiesToReparent)
		{
			state.EntityManager.AddComponentData(entity, new Parent { Value = target.target });
			state.EntityManager.SetComponentData(entity, LocalTransform.FromPosition(target.localPosition));
		}
	}
}
