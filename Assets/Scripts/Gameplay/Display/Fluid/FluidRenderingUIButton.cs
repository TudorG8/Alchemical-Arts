using AlchemicalArts.Gameplay.Display.Fluid.Models;
using AlchemicalArts.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(HighlightedImage))]
public class FluidRenderingUIButton : MonoBehaviour
{
	[field: SerializeField]
	public FluidRenderingMode Mode { get; private set; }

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