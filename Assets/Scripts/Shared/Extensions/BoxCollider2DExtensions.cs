using System.Collections.Generic;
using UnityEngine;

namespace PotionCraft.Shared.Extensions
{
	public static class BoxCollider2DExtensions
	{
		public static IEnumerable<Vector2> ToLocalCorners(this BoxCollider2D input, Vector2 offset)
		{
			var half = input.size * 0.5f;

			yield return new(-half.x + offset.x, -half.y + offset.y);
			yield return new(-half.x + offset.x, half.y + offset.y);
			yield return new(half.x + offset.x, half.y + offset.y);
			yield return new(half.x + offset.x, -half.y + offset.y);
		}
		
		public static IEnumerable<Vector2> ToLocalCorners(this BoxCollider2D input)
		{
			return input.ToLocalCorners(input.offset);
		}

		public static IEnumerable<Vector2> ApplyMatrix(this IEnumerable<Vector2> input, Matrix4x4 matrix)
		{
			foreach(var vector in input)
			{
				yield return matrix.MultiplyPoint3x4(vector);
			}
		}
	}
}