using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class Vector2Extensions
	{
		public static Vector3 To3D(this Vector2 input)
		{
			return input;
		}

		public static float3 ToFloat3(this Vector2 input)
		{
			return new float3(input.x, input.y, 0);
		}
	}
}