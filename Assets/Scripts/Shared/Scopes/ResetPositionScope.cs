using System;
using UnityEngine;

namespace AlchemicalArts.Shared.Scopes
{
	public class ResetPositionScope : IDisposable
	{
		private readonly Transform input;

		private readonly Vector3 position;

		public ResetPositionScope(Transform input)
		{
			this.input = input;
			this.position = input.transform.position;

			input.transform.position = Vector3.zero;
		}

		public void Dispose()
		{
			input.transform.position = position;
		}
	}
}