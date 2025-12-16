using Unity.Entities;

namespace PotionCraft.Core.Naming.Components
{
	public struct FolderManagerConfig : IComponentData
	{
		public Entity LiquidFolder;

		public Entity BuildingFolder;
	}
}