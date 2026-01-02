using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AlchemicalArts.Shared.Extensions
{
	public static class AwaitableExtensions
	{
		public static IEnumerator ToCoroutine<T>(this Awaitable<T> awaitable, Action<T> resultHandler)
		{
			return awaitable.AsUniTask().ToCoroutine(resultHandler);
		}
	}
}