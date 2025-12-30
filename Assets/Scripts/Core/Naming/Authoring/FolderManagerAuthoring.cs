using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Naming.Authoring
{
	public struct FolderManagerData : IComponentData
	{
		public Entity FluidFolder;

		public Entity BuildingFolder;
	}

	public class FolderManagerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		private Transform FluidFolder { get; set; }

		[field: SerializeField]
		private Transform BuildingFolder { get; set; }


		public class FolderManagerAuthoringBaker : Baker<FolderManagerAuthoring>
		{
			public override void Bake(FolderManagerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new FolderManagerData()
				{
					FluidFolder = GetEntity(authoring.FluidFolder, TransformUsageFlags.Dynamic),
					BuildingFolder = GetEntity(authoring.BuildingFolder, TransformUsageFlags.Dynamic),
				});
			}
		}
	}
}