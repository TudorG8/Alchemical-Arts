using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class TaskExtensions
	{
		public static IEnumerator ToCoroutine(this Task task)
		{
			while (!task.IsCompleted)
			{
				yield return new WaitForEndOfFrame();
			}
		}
	}
}