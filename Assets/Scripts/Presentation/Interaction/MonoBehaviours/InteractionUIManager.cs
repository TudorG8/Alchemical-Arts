using System.Collections.Generic;
using UnityEngine;

namespace AlchemicalArts.Presentation.Interaction.MonoBehaviours
{
	public class InteractionUIManager : MonoBehaviour
	{
		[System.Serializable]
		private class InteractionUIPair
		{
			[field: SerializeField]
			public InteractionUIButton Button { get; private set; }

			[field: SerializeField]
			public Transform Panel { get; private set; }
		}


		[field: SerializeField]
		private List<InteractionUIPair> Buttons { get; set; }

		[field: SerializeField]
		private InteractionService InteractionService { get; set; }


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
			foreach(var button in Buttons)
			{
				button.Button.Button.onClick.AddListener(() => InteractionService.SetInteractionMode(button.Button.Mode));
				button.Panel.gameObject.SetActive(false);
			}
		}

		private void UpdateElementState()
		{
			if (!InteractionService.InteractionModeQuery.TryGetSingleton<InteractionModeState>(out var interactionModeState))
			{
				return;
			}

			foreach(var button in Buttons)
			{
				var isActive = button.Button.Mode == interactionModeState.mode;
				button.Panel.gameObject.SetActive(isActive);
				button.Button.HighlightedImage.Highlight.color = isActive
					? button.Button.HighlightedImage.Active
					: button.Button.HighlightedImage.Idle;
			}
		}
	}
}