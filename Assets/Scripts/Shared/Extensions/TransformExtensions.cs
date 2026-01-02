using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Shared.Extensions
{
	public static class TransformExtensions
	{
		public static float ToAngle(this Transform input)
		{
			return input.eulerAngles.z * Mathf.Deg2Rad;
		}

		public static PhysicsRotate ToPhysicsRotate(this Transform input)
		{
			return new PhysicsRotate(input.ToAngle());
		}

		public static PhysicsTransform ToPhysicsTransform(this Transform input)
		{
			return new PhysicsTransform(input.position, input.ToPhysicsRotate());
		}

		public static float ToCircleScale(this Transform input)
		{
			return Mathf.Max(input.lossyScale.x, input.lossyScale.y);
		}
	}
}