using Unity.Entities;
using UnityEngine;

class MainCameraAuthoring : MonoBehaviour
{
	
}

class MainCameraAuthoringBaker : Baker<MainCameraAuthoring>
{
	public override void Bake(MainCameraAuthoring authoring)
	{
		var entity = GetEntity(TransformUsageFlags.None);
		AddComponent(entity, new MainCameraTag());
		AddComponent(entity, new InitializeMainCameraTag());
	}
}
