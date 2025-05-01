using System;
using System.Threading;
using UnityEngine;

namespace PotionCraft.Core.Utility
{
	public static class TaskUtility
	{
		public static async void EveryFrame(CancellationToken token, Action action)
		{
			await Awaitable.NextFrameAsync();

			while (!token.IsCancellationRequested)
			{
				action();
				await Awaitable.NextFrameAsync();
			}
		}
	}
}