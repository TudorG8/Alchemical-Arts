using UnityEngine;
using UnityEngine.UI;

public class HighlightedImage : MonoBehaviour
{
	[field: SerializeField]
	public Image Highlight { get; private set; }

	[field: SerializeField]
	public Color Idle { get; private set; }

	[field: SerializeField]
	public Color Active { get; private set; }
}