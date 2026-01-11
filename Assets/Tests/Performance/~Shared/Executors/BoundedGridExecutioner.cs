using System;
using UnityEngine;

namespace AlchemicalArts.Tests.Performance.Shared.Executors
{
	public class BoundedGridExecutioner
	{
		private readonly Action<Vector2> invoke;

		private readonly int count;

		private readonly Vector2 bounds;

		private readonly Vector2 position;


		public BoundedGridExecutioner(Action<Vector2> invoke, Vector2 position, Vector2 bounds, int count)
		{
			this.invoke = invoke;
			this.position = position;
			this.bounds = bounds;
			this.count = count;
		}


		public void Execute()
		{
			int rows = Mathf.CeilToInt(Mathf.Sqrt(count));
			int cols = rows;

			var min = position + -bounds * 0.5f;
			var max = position +  bounds * 0.5f;

			var stepX = cols > 1 ? (max.x - min.x) / (cols - 1) : 0f;
			var stepY = rows > 1 ? (max.y - min.y) / (rows - 1) : 0f;

			var invoked = 0;

			for (int row = 0; row < rows; row++)
			{
				float y = min.y + stepY * row;

				for (int col = 0; col < cols; col++)
				{
					float x = min.x + stepX * col;
					invoke(new Vector2(x, y));
					invoked++;
				}

				if (invoked >= count)
					break;
			}
		}
	}
}