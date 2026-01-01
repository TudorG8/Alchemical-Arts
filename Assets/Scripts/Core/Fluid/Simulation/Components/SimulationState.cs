using Unity.Entities;

namespace PotionCraft.Core.Fluid.Simulation.Components
{
	[System.Serializable]
	public struct SimulationState : IComponentData
	{
		public float gravity;

		public int predictionFrames;

		public float radius;

		public float targetDensity;

		public float pressureMultiplier;

		public float nearPressureMultiplier;

		public float viscosityStrength;
	}
}