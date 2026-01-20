using AlchemicalArts.Presentation.FluidRendering.Models;
using Unity.Entities;

namespace AlchemicalArts.Presentation.FluidRendering.Components
{
	[System.Serializable]
	public struct FluidRenderingState : IComponentData
	{
		public FluidRenderingMode mode;
	}
}