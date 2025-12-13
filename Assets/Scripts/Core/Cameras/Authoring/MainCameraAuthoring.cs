using PotionCraft.Core.Cameras.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Cameras.Authoring
{
	class MainCameraAuthoring : MonoBehaviour
	{
		
	}

	class MainCameraAuthoringBaker : Baker<MainCameraAuthoring>
	{
		public override void Bake(MainCameraAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new MainCameraTag());
			AddComponent(entity, new MainCameraInitializeTag());
		}
	}
}