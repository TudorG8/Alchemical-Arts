using Unity.Entities;

namespace PotionCraft.Core.SpatialPartioning.Components
{
	[System.Serializable]
	public struct SimulationConfig : IComponentData
	{
		public int predictionFrames;

		public float radius;

		public float gravity;

		public float targetDensity;

		public float pressureMultiplier;

		public float nearPressureMultiplier;

		public float viscosityStrength;
	}
}