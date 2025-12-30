using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Components
{
	public enum DraggingParticlesMode
	{
		Idle, Inwards, Outwards
	}
	
	[System.Serializable]
	public struct MinMaxFloatValue
	{
		public float minimum;

		public float maximum;
	}

	public static class MinMaxFloatValueExtensions
	{
		public static float Clamp(this MinMaxFloatValue input, float value)
		{
			return math.clamp(value, input.minimum, input.maximum);
		}
	}

	public struct DraggingParticlesModeState : IComponentData
	{
		public DraggingParticlesMode mode;
	}

	public struct FluidInputState : IComponentData
	{
		public MinMaxFloatValue interactionRadiusBounds;

		public float scrollSpeed;

		public float damping;

		public float interactionStrength;
	}

	public struct FluidInputConfig : IComponentData
	{
		public float interactionRadius;

		public float2 position;

		public Entity target;
	}
}