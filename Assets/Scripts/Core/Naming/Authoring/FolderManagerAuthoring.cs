using PotionCraft.Core.Naming.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Naming.Authoring
{
	public class FolderManagerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public Transform FluidFolder { get; set; }

		[field: SerializeField]
		public Transform BuildingFolder { get; set; }
	}

	public class FolderManagerAuthoringBaker : Baker<FolderManagerAuthoring>
	{
		public override void Bake(FolderManagerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FolderManagerConfig()
			{
				FluidFolder = GetEntity(authoring.FluidFolder, TransformUsageFlags.Dynamic),
				BuildingFolder = GetEntity(authoring.BuildingFolder, TransformUsageFlags.Dynamic),
			});
		}
	}
}