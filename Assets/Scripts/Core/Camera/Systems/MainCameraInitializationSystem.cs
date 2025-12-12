using PotionCraft.Core.Camera.Groups;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(CameraInitializationGroup))]
partial struct MainCameraInitializationSystem : ISystem
{
	private EntityQuery cameraQuery;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		cameraQuery = SystemAPI.QueryBuilder().WithAll<MainCameraTag>().WithAll<InitializeMainCameraTag>().Build();
		state.RequireForUpdate(cameraQuery);
	}
	
	public void OnUpdate(ref SystemState state)
	{
		var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

		var camera = cameraQuery.GetSingletonEntity();
		var cameraReference = new CameraReference() { Camera = new UnityObjectRef<Camera>() { Value = Camera.main}};
		commandBuffer.RemoveComponent<InitializeMainCameraTag>(camera);
		commandBuffer.AddComponent(camera, cameraReference);
	}
}
