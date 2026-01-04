using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using AlchemicalArts.Shared.Extensions;

namespace AlchemicalArts.Core.Physics.Extensions
{
	public static class PhysicsTransformExtensions
	{
		public static PhysicsTransform MultiplyBy(this PhysicsTransform input, float value)
		{
			return new PhysicsTransform(value * input.position, new PhysicsRotate(value * input.rotation.angle));
		}

		public static Matrix4x4 ToMatrix(this PhysicsTransform input)
		{
			var offsetMatrix = Matrix4x4.Translate(input.position);
			var rotateMatrix = Matrix4x4.Rotate(input.rotation.angle.ToQuaternion());
			return rotateMatrix * offsetMatrix;
		}
	}
}