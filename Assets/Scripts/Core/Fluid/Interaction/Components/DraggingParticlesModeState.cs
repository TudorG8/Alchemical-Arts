using AlchemicalArts.Core.Fluid.Interaction.Models;
using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Interaction.Components
{
	public struct DraggingParticlesModeState : IComponentData
	{
		public DraggingParticlesMode mode;
	}
}