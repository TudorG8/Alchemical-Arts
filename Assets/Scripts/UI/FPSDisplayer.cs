using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Unity.Entities;
using TMPro;

public class FPSDisplayer : MonoBehaviour
{
	public TextMeshProUGUI FPSAmount;

	private void Start()
	{
		FPSLoop();
	}

	private async void FPSLoop()
	{
		while (true)
		{
			FPSAmount.text = $"{Mathf.FloorToInt(1f / Time.unscaledDeltaTime)}";

			await Awaitable.WaitForSecondsAsync(0.05f);
		}
	}
}