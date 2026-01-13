using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Entities;

namespace AlchemicalArts.Core.Fluid.Simulation.Components
{
	public struct FluidPartionedIndex : IComponentData, IIndexedComponent
	{
		public int index;

		public int Index { get => index; set => index = value;}
	}
}