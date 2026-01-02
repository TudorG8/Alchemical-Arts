using PotionCraft.Core.Naming.BakingComponents;
using PotionCraft.Core.Naming.Components;
using PotionCraft.Shared.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Naming.BakingSystems
{
	[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
	public partial struct EntityHierarchyBakingSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (transformLinkBuffer, entity) in SystemAPI.Query<DynamicBuffer<TransformLinkBakingData>>()
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.WithEntityAccess())
			{
				foreach(var transformLink in transformLinkBuffer)
				{
					commandBuffer.AddComponent(transformLink.Child, new EntityNameConfig { value = transformLink.Name });
					if (transformLink.Parent != Entity.Null)
					{
						ParentUtility.ReparentLocalPosition(ref state, commandBuffer, transformLink.Child, transformLink.Parent);
					}
				}
				commandBuffer.RemoveComponent<TransformLinkBakingData>(entity);
			}
			
			commandBuffer.Playback(state.EntityManager);
		}
	}
}