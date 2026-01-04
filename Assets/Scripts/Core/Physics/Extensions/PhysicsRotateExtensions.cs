using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Extensions
{
	public static class PhysicsRotateExtensions
	{
		public static Quaternion ToQuaternion(this PhysicsRotate input)
		{
			return Quaternion.Euler(0f, 0f, -input.angle * Mathf.Rad2Deg);
		}

		public static quaternion ToECSQuaternion(this PhysicsRotate input)
		{
			return quaternion.RotateZ(input.angle);
		}
	}
}