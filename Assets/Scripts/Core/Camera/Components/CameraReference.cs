using Unity.Entities;

namespace PotionCraft.Core.Camera.Components
{
	public struct CameraReference : IComponentData
	{
		public UnityObjectRef<UnityEngine.Camera> Camera;
	}
}