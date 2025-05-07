using Unity.Entities;
using UnityEngine;

public struct _FolderManagerData : IComponentData
{
	public Entity LiquidFolder;
}

class FolderManagerAuthoring : MonoBehaviour
{
	[field: SerializeField]
	public Transform LiquidFolder { get; private set; }


	class FolderManagerAuthoringBaker : Baker<FolderManagerAuthoring>
	{
		public override void Bake(FolderManagerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new _FolderManagerData()
			{
				LiquidFolder = GetEntity(authoring.LiquidFolder, TransformUsageFlags.Dynamic)
			});
		}
	}
}