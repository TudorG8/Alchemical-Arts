using AlchemicalArts.Gameplay.Tools.FluidInteraction.Models;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.Components
{
	public struct DraggingParticlesModeState : IComponentData
	{
		public DraggingParticlesMode mode;
	}
}