using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class Vector3Extensions
	{
		public static Vector2 To2D(this Vector3 input)
		{
			return input;
		}

		public static float2 Tofloat2(this Vector3 input)
		{
			return new float2(input.x, input.y);
		}
	}
}