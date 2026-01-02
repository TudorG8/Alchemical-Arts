using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Extensions
{
	public static class EntityExtensions
	{
		public static PhysicsMask EncodeAsPhysicsMask(this Entity entity) => UnsafeUtility.As<Entity, PhysicsMask>(ref entity);
	}
}