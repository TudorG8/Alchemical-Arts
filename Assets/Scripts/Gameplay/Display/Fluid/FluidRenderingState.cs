using AlchemicalArts.Gameplay.Display.Fluid.Models;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Display.Fluid.Components
{
	[System.Serializable]
	public struct FluidRenderingState : IComponentData
	{
		public FluidRenderingMode mode;
	}
}