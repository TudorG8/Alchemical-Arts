using Unity.Mathematics;
using UnityEngine;

namespace PotionCraft.Shared.Extensions
{
	public static class float2Extensions
	{
		public static Vector3 To3D(this float2 input, float z = 0)
		{
			return new Vector3(input.x, input.y, z);
		}

		public static float3 ToFloat3(this float2 input, float z = 0)
		{
			return new float3(input.x, input.y, z);
		}
	}
}