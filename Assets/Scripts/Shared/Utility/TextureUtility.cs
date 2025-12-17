using UnityEngine;

namespace PotionCraft.Shared.Utility
{
	public static class TextureUtility
	{
		public static Texture2D TextureFromGradient(int width, Gradient gradient, FilterMode filterMode = FilterMode.Bilinear)
		{
			var texture = new Texture2D(width, 1)
			{
				wrapMode = TextureWrapMode.Clamp,
				filterMode = filterMode
			};

			var cols = new Color[width];
			for (int i = 0; i < cols.Length; i++)
			{
				var time = i / (cols.Length - 1f);
				cols[i] = gradient.Evaluate(time);
			}

			texture.SetPixels(cols);
			texture.Apply();

			return texture;
		}
	}
}