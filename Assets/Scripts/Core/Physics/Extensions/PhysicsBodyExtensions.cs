using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Extensions
{
	public static class PhysicsBodyExtensions
	{
		public static PhysicsTransform ToPhysicsTransform(this PhysicsBody input)
		{
			return new PhysicsTransform(input.position, input.rotation);
		}

		public static PhysicsTransform ToReversePhysicsTransform(this PhysicsBody input)
		{
			var angle = (360 - input.rotation.angle * math.TODEGREES) * math.TORADIANS;
			return new PhysicsTransform(-input.position, new PhysicsRotate(angle));
		}
	}
}