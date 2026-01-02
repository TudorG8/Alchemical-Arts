namespace AlchemicalArts.Core.SpatialPartioning.Utility
{
	public static class SpatialWeightingUtility
	{
		public static float ComputeSpikyPow3Weight(float spikyPow3ScalingFactor, float distance, float radius)
		{
			if (distance < radius)
			{
				float v = radius - distance;
				return v * v * v * spikyPow3ScalingFactor;
			}
			return 0;
		}

		public static float ComputeSpikyPow2Weight(float spikyPow2ScalingFactor, float distance, float radius)
		{
			if (distance < radius)
			{
				float v = radius - distance;
				return v * v * spikyPow2ScalingFactor;
			}
			return 0;
		}

		public static float ComputeDerivativeSpikyPow2(float spikyPow2DerivativeScalingFactor, float distance, float radius)
		{
			if (distance <= radius)
			{
				float v = radius - distance;
				return -v * spikyPow2DerivativeScalingFactor;
			}
			return 0;
		}

		public static float ComputeDerivativeSpikyPow3(float spikyPow3DerivativeScalingFactor, float distance, float radius)
		{
			if (distance <= radius)
			{
				float v = radius - distance;
				return -v * v * spikyPow3DerivativeScalingFactor;
			}
			return 0;
		}

		public static float ComputeSmoothingPoly6(float poly6ScalingFactor, float distance, float radius)
		{
			if (distance < radius)
			{
				float v = radius * radius - distance * distance;
				return v * v * v * poly6ScalingFactor;
			}
			return 0;
		}
	}
}