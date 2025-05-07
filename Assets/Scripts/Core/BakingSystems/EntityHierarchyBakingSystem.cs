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
		using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (transformLinkBuffer, entity) in SystemAPI.Query<DynamicBuffer<TransformLinkData>>()
			.WithOptions(EntityQueryOptions.IncludePrefab)
			.WithEntityAccess())
		{
			foreach(var transformLink in transformLinkBuffer.ToNativeArray(Allocator.Temp))
			{
				commandBuffer.AddComponent(transformLink.Child, new _EntityName { name = transformLink.Name });
				if (transformLink.Parent != Entity.Null)
				{
					ParentUtility.ReparentLocalPosition(ref state, commandBuffer, transformLink.Child, transformLink.Parent);
				}
			}
			commandBuffer.RemoveComponent<TransformLinkData>(entity);
		}
		
		commandBuffer.Playback(state.EntityManager);
	}
}
