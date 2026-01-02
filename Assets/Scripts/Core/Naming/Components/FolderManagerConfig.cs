using Unity.Entities;

namespace AlchemicalArts.Core.Naming.Components
{
	public struct FolderManagerConfig : IComponentData
	{
		public Entity fluidFolder;

		public Entity buildingFolder;
	}
}