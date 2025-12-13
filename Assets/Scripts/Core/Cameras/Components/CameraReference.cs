using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Cameras.Components
{
	public struct CameraReference : IComponentData
	{
		public UnityObjectRef<Camera> Camera;
	}
}