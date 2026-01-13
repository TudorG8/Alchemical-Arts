using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Entities;

public struct TemperaturePartionedIndex : IComponentData, IIndexedComponent
{
	public int index;

	public int Index { get => index; set => index = value;}
}
