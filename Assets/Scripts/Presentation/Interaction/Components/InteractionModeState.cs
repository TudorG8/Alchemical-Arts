using AlchemicalArts.Presentation.Interaction.Models;
using Unity.Entities;

namespace AlchemicalArts.Presentation.Interaction
{
	public struct InteractionModeState : IComponentData
	{
		public InteractionMode mode;
	}
}