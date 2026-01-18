using System.Collections.Generic;
using System.Threading;
using AlchemicalArts.Gameplay.Display.Fluid.Components;
using AlchemicalArts.Gameplay.Display.Fluid.MonoBehaviours;
using AlchemicalArts.Shared.Extensions;
using Unity.Entities;
using UnityEngine;

public class FluidRenderingUIManager : MonoBehaviour
{
	public List<FluidRenderingUIButton> buttons;

	public FluidRenderingService fluidRenderingService;

	public void Generate()
	{
		foreach(var fluidRenderingUIButton in buttons)
		{
			fluidRenderingUIButton.Button.onClick.AddListener(() => fluidRenderingService.ChangeToMode(fluidRenderingUIButton.Mode));
		}
	}


	private void Start()
	{
		Generate();
		UpdateLoop(destroyCancellationToken);
	}

	private async void UpdateLoop(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			var fluidRenderingState = await World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentDataAsync<FluidRenderingState>();

			foreach(var fluidRenderingUIButton in buttons)
			{
				fluidRenderingUIButton.HighlightedImage.Highlight.color = 
					fluidRenderingUIButton.Mode == fluidRenderingState.mode 
						? fluidRenderingUIButton.HighlightedImage.Active
						: fluidRenderingUIButton.HighlightedImage.Idle; 
			}

			await Awaitable.EndOfFrameAsync();
		}
	}
}
