using AlchemicalArts.Shared.Models;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Components
{
	public struct FluidInputConfig : IComponentData
	{
		public MinMaxFloatValue interactionRadiusBounds;

		public float scrollSpeed;

		public float damping;

		public float interactionStrength;

		public Entity target;
	}
}