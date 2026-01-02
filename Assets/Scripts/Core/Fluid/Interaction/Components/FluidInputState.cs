using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Interaction.Components
{
	public struct FluidInputState : IComponentData
	{
		public float interactionRadius;

		public float2 position;
	}
}