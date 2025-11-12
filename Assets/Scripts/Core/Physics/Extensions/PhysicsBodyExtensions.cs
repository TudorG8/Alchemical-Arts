using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Extensions
{
	public static class PhysicsBodyExtensions
	{
		public static PhysicsTransform ToPhysicsTransform(this PhysicsBody input)
		{
			return new PhysicsTransform(input.position, input.rotation);
		}
	}
}