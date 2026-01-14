using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Entities;

public struct TemperaturePartionedIndex : IComponentData, IIndexedComponent
{
	public int index;

	public int Index { readonly get => index; set => index = value;}
}
