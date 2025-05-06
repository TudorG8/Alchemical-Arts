using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

#if UNITY_EDITOR
public class SceneLoaderAuthoring : MonoBehaviour
{
	public UnityEditor.SceneAsset Scene;

	class Baker : Baker<SceneLoaderAuthoring>
	{
		public override void Bake(SceneLoaderAuthoring authoring)
		{
			var reference = new EntitySceneReference(authoring.Scene);
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponent(entity, new SceneLoader
			{
				SceneReference = reference
			});
		}
	}
}
#endif

public struct SceneLoader : IComponentData
{
	public EntitySceneReference SceneReference;
}