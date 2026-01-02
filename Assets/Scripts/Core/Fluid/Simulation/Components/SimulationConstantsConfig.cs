using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Components
{
	public struct SimulationConstantsConfig : IComponentData
	{
		public FixedList128Bytes<int2> offsets;

		public float spikyPow3ScalingFactor;

		public float spikyPow2ScalingFactor;

		public float spikyPow2DerivativeScalingFactor;

		public float spikyPow3DerivativeScalingFactor;

		public float poly6ScalingFactor;
	}
}