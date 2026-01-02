using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Simulation.Components
{
	[System.Serializable]
	public struct FluidSimulationConfig : IComponentData
	{
		public float gravity;

		public float targetDensity;

		public float pressureMultiplier;

		public float nearPressureMultiplier;

		public float viscosityStrength;
	}
}