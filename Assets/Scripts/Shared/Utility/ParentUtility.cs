using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PotionCraft.Shared.Utility
{
	public static class ParentUtility
	{
		public static void ReparentLocalPosition(ref SystemState state, EntityCommandBuffer commandBuffer, Entity child, Entity parent)
		{
			var localTransform = state.EntityManager.GetComponentData<LocalTransform>(child);
			commandBuffer.AddComponent(child, new Parent { Value = parent });
			commandBuffer.SetComponent(child, localTransform);
		}
	}
}