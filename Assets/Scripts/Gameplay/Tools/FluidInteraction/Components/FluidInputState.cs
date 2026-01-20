using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Components
{
	public struct FluidInputState : IComponentData
	{
		public float interactionRadius;

		public float2 position;
	}
}