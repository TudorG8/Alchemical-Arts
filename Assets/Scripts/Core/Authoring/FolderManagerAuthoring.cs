using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Authoring
{
	public struct _FolderManagerData : IComponentData
	{
		public Entity LiquidFolder;

		public Entity BuildingFolder;
	}

	public class FolderManagerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		private Transform LiquidFolder { get; set; }

		[field: SerializeField]
		private Transform BuildingFolder { get; set; }


		public class FolderManagerAuthoringBaker : Baker<FolderManagerAuthoring>
		{
			public override void Bake(FolderManagerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new _FolderManagerData()
				{
					LiquidFolder = GetEntity(authoring.LiquidFolder, TransformUsageFlags.Dynamic),
					BuildingFolder = GetEntity(authoring.BuildingFolder, TransformUsageFlags.Dynamic),
				});
			}
		}
	}
}