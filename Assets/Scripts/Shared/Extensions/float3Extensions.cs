using Unity.Mathematics;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class float3Extensions
	{
		public static Vector2 To2D(this float3 input)
		{
			return new Vector2(input.x, input.y);
		}

		public static float2 ToFloat2(this float3 input)
		{
			return new float2(input.x, input.y);
		}
	}
}