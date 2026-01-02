using Unity.Entities;

namespace PotionCraft.Core.SpatialPartioning.Components
{
	[System.Serializable]
	public struct SpatialPartioningConfig : IComponentData
	{
		public int predictionFrames;

		public float radius;
	}
}