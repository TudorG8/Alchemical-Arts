using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Entities;

namespace AlchemicalArts.Gameplay.Temperature.Components
{
	public struct TemperaturePartionedIndex : IComponentData, IIndexedComponent
	{
		public int index;

		public int Index { readonly get => index; set => index = value;}
	}
}