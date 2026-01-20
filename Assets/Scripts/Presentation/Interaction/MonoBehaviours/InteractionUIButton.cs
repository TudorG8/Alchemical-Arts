using AlchemicalArts.Presentation.Interaction.Models;
using AlchemicalArts.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace AlchemicalArts.Presentation.Interaction.MonoBehaviours
{
	[RequireComponent(typeof(Button))]
	[RequireComponent(typeof(HighlightedImage))]
	public class InteractionUIButton : MonoBehaviour
	{
		[field: SerializeField]
		public InteractionMode Mode { get; private set; }

		[field: SerializeField]
		public Button Button { get; private set; }

		[field: SerializeField]
		public HighlightedImage HighlightedImage { get; private set; }


		private void OnValidate()
		{
			Button = this.ValidateComponent(Button);
			HighlightedImage = this.ValidateComponent(HighlightedImage);
		}
	}
}