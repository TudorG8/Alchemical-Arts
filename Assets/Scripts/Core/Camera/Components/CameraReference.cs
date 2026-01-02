using Unity.Entities;

namespace AlchemicalArts.Core.Camera.Components
{
	public struct CameraReference : IComponentData
	{
		public UnityObjectRef<UnityEngine.Camera> Camera;
	}
}