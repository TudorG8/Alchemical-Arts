using PotionCraft.Core.Camera.Components;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Camera.Authoring
{
	public class MainCameraAuthoring : MonoBehaviour
	{
		
	}

	public class MainCameraAuthoringBaker : Baker<MainCameraAuthoring>
	{
		public override void Bake(MainCameraAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new MainCameraTag());
			AddComponent(entity, new MainCameraInitializeTag());
		}
	}
}