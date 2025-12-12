using System.Threading;
using UnityEngine;

namespace PotionCraft.Gameplay.Behaviours
{
	public class LiquidSpawnerBehaviour : MonoBehaviour
	{
		[SerializeField]
		private Transform parent;

		[SerializeField]
		private int counter = 0;

		private WrigglerBehaviour wriggler;

		private readonly CancellationTokenSource taskCancellation = new();


		private void Start()
		{
			wriggler = GetComponentInParent<WrigglerBehaviour>();
			Generate(taskCancellation.Token);
		}

		private void OnDestroy()
		{
			taskCancellation.Cancel();
		}

		private async void Generate(CancellationToken token)
		{
			while (counter < wriggler.LimitPerSpawner)
			{
				if (token.IsCancellationRequested)
					break;

				var obj = Instantiate(wriggler.Liquid, transform.position, Quaternion.identity);
				obj.transform.SetParent(parent);
				counter++;

				if (wriggler.SpawnerDelay <= 0)
				{
					await Awaitable.EndOfFrameAsync();
				}
				else
				{
					await Awaitable.WaitForSecondsAsync(wriggler.SpawnerDelay);
				}
			}
		}
	}
}