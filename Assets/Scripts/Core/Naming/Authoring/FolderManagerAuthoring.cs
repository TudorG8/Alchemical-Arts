using AlchemicalArts.Core.Naming.Components;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Naming.Authoring
{
	public class FolderManagerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public Transform FluidFolder { get; private set; }

		[field: SerializeField]
		public Transform BuildingFolder { get; private set; }
	}

	public class FolderManagerAuthoringBaker : Baker<FolderManagerAuthoring>
	{
		public override void Bake(FolderManagerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new FolderManagerConfig()
			{
				fluidFolder = GetEntity(authoring.FluidFolder, TransformUsageFlags.Dynamic),
				buildingFolder = GetEntity(authoring.BuildingFolder, TransformUsageFlags.Dynamic),
			});
		}
	}
}