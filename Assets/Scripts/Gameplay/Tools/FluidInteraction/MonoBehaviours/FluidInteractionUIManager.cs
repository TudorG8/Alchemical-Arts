using System.Collections.Generic;
using System.Linq;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Components;
using AlchemicalArts.Gameplay.Tools.FluidInteraction.Data;
using AlchemicalArts.Shared.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AlchemicalArts.Gameplay.Tools.FluidInteraction.MonoBehaviours
{
	public class FluidInteractionUIManager : MonoBehaviour
	{
		[field: SerializeField]
		private List<FluidDragSettings> DragSettings { get; set; }

		[field: SerializeField]
		private TextMeshProUGUI ModeText { get; set; }

		[field: SerializeField]
		private Slider SizeSlider { get; set; }
		
		[field: SerializeField]
		private FluidInteractionService FluidInteractionService { get; set; }


		private void Start()
		{
			HookupElements();
			UpdateElementState();
		}

		private void Update()
		{
			UpdateElementState();
		}

		private void HookupElements()
		{
			SizeSlider.onValueChanged.AddListener(FluidInteractionService.SetInteractionRadiusToPercentage);
		}

		private void UpdateElementState()
		{
			if (!FluidInteractionService.FluidInputQuery.TryGetSingleton<DraggingParticlesModeState>(out var draggingParticlesModeState) ||
				!FluidInteractionService.FluidInputQuery.TryGetSingleton<FluidInputState>(out var fluidInputState) ||
				!FluidInteractionService.FluidInputQuery.TryGetSingleton<FluidInputConfig>(out var fluidInputConfig))
			{
				return;
			}

			var dragSetting = DragSettings.FirstOrDefault(d => d.Mode == draggingParticlesModeState.mode);
			if (dragSetting != null)
			{
				ModeText.text = dragSetting.DisplayText.GetLocalizedString();
				ModeText.color = dragSetting.Color;
			}

			SizeSlider.SetValueWithoutNotify(fluidInputConfig.interactionRadiusBounds.Percentage(fluidInputState.interactionRadius));
		}
	}
}