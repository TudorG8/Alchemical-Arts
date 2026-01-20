using System.Collections.Generic;
using System.Linq;
using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Interaction.Models;
using AlchemicalArts.Shared.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FluidInteractionUIManager : MonoBehaviour
{
	[System.Serializable]
	private class FluidDragSettings : ScriptableObject
	{
		[field:SerializeField]
		public DraggingParticlesMode Mode { get; private set; }

		[field:SerializeField]
		public string DisplayText { get; private set; }

		[field:SerializeField]
		public Color Color { get; private set; }
	}


	[field: SerializeField]
	private List<FluidDragSettings> DragSettings { get; set; }

	[field: SerializeField]
	private TextMeshProUGUI ModeText { get; set; }

	[field: SerializeField]
	private Slider SizeSlider { get; set; }
	
	[field: SerializeField]
	private FluidInteractionService FluidInteractionService { get; set; }


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
			ModeText.text = dragSetting.DisplayText;
			ModeText.color = dragSetting.Color;
		}

		SizeSlider.SetValueWithoutNotify(fluidInputConfig.interactionRadiusBounds.Percentage(fluidInputState.interactionRadius));
	}
}