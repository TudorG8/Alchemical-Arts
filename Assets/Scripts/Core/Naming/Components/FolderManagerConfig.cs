using Unity.Entities;

namespace PotionCraft.Core.Naming.Components
{
	public struct FolderManagerConfig : IComponentData
	{
		public Entity fluidFolder;

		public Entity buildingFolder;
	}
}