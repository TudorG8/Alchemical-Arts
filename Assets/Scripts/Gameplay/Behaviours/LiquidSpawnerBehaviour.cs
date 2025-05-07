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
			while (counter < wriggler.limitPerSpawner)
			{
				if (token.IsCancellationRequested)
					break;

				var obj = Instantiate(wriggler.liquid, transform.position, Quaternion.identity);
				obj.transform.SetParent(parent);
				counter++;

				if (wriggler.spawnerDelay <= 0)
				{
					await Awaitable.EndOfFrameAsync();
				}
				else
				{
					await Awaitable.WaitForSecondsAsync(wriggler.spawnerDelay);
				}
			}
		}
	}
}