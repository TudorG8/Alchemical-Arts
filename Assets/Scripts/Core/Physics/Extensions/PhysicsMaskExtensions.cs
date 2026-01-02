using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Extensions
{
	public static class PhysicsMaskExtensions
	{
		public static Entity DecodeFromPhysicsMask(this PhysicsMask physicsMask) => UnsafeUtility.As<PhysicsMask, Entity>(ref physicsMask);
	}
}