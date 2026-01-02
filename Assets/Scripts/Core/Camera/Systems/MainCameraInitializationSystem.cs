using AlchemicalArts.Core.Camera.Components;
using AlchemicalArts.Core.Camera.Groups;
using Unity.Burst;
using Unity.Entities;

namespace AlchemicalArts.Core.Camera.Systems
{
	[UpdateInGroup(typeof(CameraInitializationGroup))]
	public partial struct MainCameraInitializationSystem : ISystem
	{
		private EntityQuery cameraQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			cameraQuery = SystemAPI.QueryBuilder().WithAll<MainCameraTag>().WithAll<MainCameraInitializeTag>().Build();
			state.RequireForUpdate(cameraQuery);
		}
		
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			var camera = cameraQuery.GetSingletonEntity();
			var cameraReference = new CameraReference() { Camera = new UnityObjectRef<UnityEngine.Camera>() { Value = UnityEngine.Camera.main}};
			commandBuffer.RemoveComponent<MainCameraInitializeTag>(camera);
			commandBuffer.AddComponent(camera, cameraReference);
		}
	}
}