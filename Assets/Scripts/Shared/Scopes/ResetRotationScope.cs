
using System;
using UnityEngine;

namespace AlchemicalArts.Shared.Scopes
{
	public class ResetRotationScope : IDisposable
	{
		private readonly Transform input;

		private readonly Quaternion rotation;

		public ResetRotationScope(Transform input)
		{
			this.input = input;
			this.rotation = input.transform.rotation;

			input.transform.rotation = Quaternion.identity;
		}

		public void Dispose()
		{
			input.transform.rotation = rotation;
		}
	}
}