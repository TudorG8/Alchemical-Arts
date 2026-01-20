using System.Collections.Generic;
using AlchemicalArts.Presentation.FluidRendering.Components;
using UnityEngine;

namespace AlchemicalArts.Presentation.FluidRendering.MonoBehaviours
{
	public class FluidRenderingUIManager : MonoBehaviour
	{
		[field:SerializeField]
		private List<FluidRenderingUIButton> Buttons { get; set; }

		[field:SerializeField]
		private FluidRenderingService FluidRenderingService { get; set;}


		private void Awake()
		{
			HookupElements();
		}

		private void Update()
		{
			UpdateElementState();
		}

		private void HookupElements()
		{
			foreach(var fluidRenderingUIButton in Buttons)
			{
				fluidRenderingUIButton.Button.onClick.AddListener(() => FluidRenderingService.SetRenderingMode(fluidRenderingUIButton.Mode));
			}
		}

		private void UpdateElementState()
		{
			if (!FluidRenderingService.FluidRenderingQuery.TryGetSingleton<FluidRenderingState>(out var fluidRenderingState))
			{
				return;
			}

			foreach(var fluidRenderingUIButton in Buttons)
			{
				fluidRenderingUIButton.HighlightedImage.Highlight.color = 
					fluidRenderingUIButton.Mode == fluidRenderingState.mode 
						? fluidRenderingUIButton.HighlightedImage.Active
						: fluidRenderingUIButton.HighlightedImage.Idle; 
			}
		}
	}
}
