using AlchemicalArts.Gameplay.Tools.FluidInteraction.Components;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Models;
using AlchemicalArts.Shared.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.MonoBehaviours
{
	public class FluidInteractionTool : MonoBehaviour
	{
		[field: SerializeField]
		private GameInput GameInput { get; set;}

		[field: SerializeField]
		private Camera Camera { get; set;}

		[field: SerializeField]
		private FluidInteractionService FluidInteractionService { get; set; }



		private void OnEnable()
		{
			GameInput = new GameInput();
			GameInput.Enable();
		}

		
		private void OnDisable()
		{
			GameInput.Disable();
			GameInput.Dispose();

			FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Inactive);
		}

		void Update()
		{
			if (!FluidInteractionService.FluidInputQuery.TryGetSingleton<DraggingParticlesModeState>(out var draggingParticlesModeState))
			{
				return;
			}

			var primaryPressed = GameInput.Player.Primary.WasPressedThisFrame();
			var primaryHeld = GameInput.Player.Primary.IsPressed();
			var secondaryPressed = GameInput.Player.Secondary.WasPressedThisFrame();
			var secondaryHeld = GameInput.Player.Secondary.IsPressed();
			var screenPosition = GameInput.Player.Position.ReadValue<Vector2>();
			var scrollDelta = GameInput.Player.Scroll.ReadValue<float>();

			FluidInteractionService.SetInteractionPosition(Camera.ScreenToWorldPoint(screenPosition.To3D()).To2D());

			// Handling letting go even over UI
			switch (draggingParticlesModeState.mode)
			{
				case DraggingParticlesMode.Inwards when !primaryHeld: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Idle); break;
				case DraggingParticlesMode.Outwards when !secondaryHeld: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Idle); break;
			}

			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			switch (draggingParticlesModeState.mode)
			{
				// Starting interaction
				case DraggingParticlesMode.Inactive: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Idle); break;
				case DraggingParticlesMode.Idle when primaryPressed: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Inwards); break;
				case DraggingParticlesMode.Idle when secondaryPressed: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Outwards); break;

				// Swapping Modes midway
				case DraggingParticlesMode.Outwards when primaryPressed: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Inwards); break;
				case DraggingParticlesMode.Inwards when secondaryPressed: FluidInteractionService.SetInteractionMode(DraggingParticlesMode.Outwards); break;
			}

			if (draggingParticlesModeState.mode == DraggingParticlesMode.Idle)
			{
				FluidInteractionService.ApplyScrollDelta(scrollDelta);
				return;
			}
		}
	}
}