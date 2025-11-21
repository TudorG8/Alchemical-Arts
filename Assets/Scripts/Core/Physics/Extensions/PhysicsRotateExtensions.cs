
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Extensions
{
	public static class PhysicsRotateExtensions
	{
		public static Quaternion ToQuaternion(this PhysicsRotate input)
		{
			return Quaternion.Euler(0f, 0f, -input.angle * Mathf.Rad2Deg);
		}

		public static Quaternion ToQuaternion(this float input)
		{
			return Quaternion.Euler(0f, 0f, input * Mathf.Rad2Deg);
		}

		public static quaternion ToECSQuaternion(this PhysicsRotate input)
		{
			return quaternion.RotateZ(input.angle);
		}

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