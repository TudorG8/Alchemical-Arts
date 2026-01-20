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

		public static float Percentage(this MinMaxFloatValue input, float value)
		{
			return (value - input.minimum) / (input.maximum - input.minimum);
		}

		public static float PercentageValue(this MinMaxFloatValue input, float percentage)
		{
			return input.minimum + (input.maximum - input.minimum) * percentage;
		}
	}
}