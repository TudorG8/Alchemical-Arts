using Unity.Mathematics;

namespace AlchemicalArts.Core.SpatialPartioning.Utility
{
	public static class SpatialHashingUtility
	{
		private const int PRIME_1 = 15823;

		private const int PRIME_2 = 9737333;


		public static uint HashCell2D(float2 input)
		{
			var a = (uint)input.x * PRIME_1;
			var b = (uint)input.y * PRIME_2;
			return a + b;
		}

		public static int2 GetCell2D(float2 input, float radius)
		{
			var x = (int)math.floor(input.x / radius);
			var y = (int)math.floor(input.y / radius);

			return new int2(x, y);
		}

		public static int KeyFromHash(uint hash, int length)
		{
			return (int)(hash % (uint)length);
		}
	}
}