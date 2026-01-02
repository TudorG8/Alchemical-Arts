using AlchemicalArts.Shared.Models;
using Unity.Mathematics;

namespace AlchemicalArts.Shared.Extensions
{
	public static class MinMaxFloatValueExtensions
	{
		public static float Clamp(this MinMaxFloatValue input, float value)
		{
			return math.clamp(value, input.minimum, input.maximum);
		}
	}
}