using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Components
{
	public struct SpatiallyPartionedIndex : IComponentData, IIndexedComponent
	{
		public int index;

		public int Index { get => index; set => index = value;}
	}
}