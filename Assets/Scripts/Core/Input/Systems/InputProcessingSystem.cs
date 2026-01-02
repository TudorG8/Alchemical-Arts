using PotionCraft.Core.Cameras.Components;
using PotionCraft.Core.Input.Components;
using PotionCraft.Core.Input.Groups;
using PotionCraft.Shared.Extensions;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Input.Systems
{
	[UpdateInGroup(typeof(InputSystemGroup), OrderFirst = true)]
	public partial class InputProcessingSystem : SystemBase
	{
		private GameInput gameInput;

		private EntityQuery cameraQuery;

		private EntityQuery gameInputQuery;


		protected override void OnCreate()
		{
			gameInput = new GameInput();
			gameInput.Enable();

			cameraQuery = SystemAPI.QueryBuilder().WithAll<MainCameraTag>().WithAll<CameraReference>().Build();
			gameInputQuery = SystemAPI.QueryBuilder().WithAll<InputDataConfig>().Build();
			
			RequireForUpdate(cameraQuery);
			RequireForUpdate(gameInputQuery);
		}

		protected override void OnDestroy()
		{
			gameInput.Disable();
			gameInput.Dispose();
		}

		protected override void OnUpdate()
		{
			var inputData = gameInputQuery.GetSingleton<InputDataConfig>();
			
			inputData.primaryPressed = gameInput.Player.Primary.IsPressed();
			inputData.secondaryPressed = gameInput.Player.Secondary.IsPressed();
			inputData.screenPosition = gameInput.Player.Position.ReadValue<Vector2>();
			inputData.scrollDelta = gameInput.Player.Scroll.ReadValue<float>();

			var camera = cameraQuery.GetSingleton<CameraReference>();
			inputData.worldPosition = camera.Camera.Value.ScreenToWorldPoint(inputData.screenPosition.To3D()).To2D();
			
			SystemAPI.SetSingleton(inputData);
		}
	}
}