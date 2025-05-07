using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PotionCraft.Core.Utility
{
	public class ParentUtility : MonoBehaviour
	{
		[BurstCompile]
		public static void ReparentLocalPosition(ref SystemState state, EntityCommandBuffer commandBuffer, Entity child, Entity parent)
		{
			var localTransform = state.GetComponentLookup<LocalTransform>().GetRefRO(child).ValueRO;
			commandBuffer.AddComponent(child, new Parent { Value = parent });
			commandBuffer.SetComponent(child, localTransform);
		}
	}
}