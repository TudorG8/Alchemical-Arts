using PotionCraft.Shared.Models;
using Unity.Entities;

namespace PotionCraft.Core.Fluid.Simulation.Components
{
	public struct FluidInputConfig : IComponentData
	{
		public MinMaxFloatValue interactionRadiusBounds;

		public float scrollSpeed;

		public float damping;

		public float interactionStrength;
	}
}