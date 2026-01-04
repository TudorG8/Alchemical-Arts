using System.Collections.Generic;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
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
	}
}