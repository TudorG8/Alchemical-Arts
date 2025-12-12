using Unity.Entities;
using UnityEngine;

public struct CameraReference : IComponentData
{
	public UnityObjectRef<Camera> Camera;
}