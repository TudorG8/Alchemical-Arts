using System;
using System.Threading;
using AlchemicalArts.Shared.Utility;
using Unity.PerformanceTesting;
using UnityEngine;

namespace AlchemicalArts.Tests.Performance.Shared.Scopes
{
	public class FramesPerSecondScope : IDisposable
	{
		private readonly CancellationTokenSource cancelSource;

		private readonly SampleGroup sampleGroup;


		public FramesPerSecondScope(string name = "Frames Per Second")
		{
			cancelSource = new CancellationTokenSource ();
			sampleGroup = new SampleGroup(name, SampleUnit.Undefined);

			TakeMeasurement();
			TaskUtility.EveryFrame(cancelSource.Token, TakeMeasurement);
		}


		public void Dispose()
		{
			cancelSource.Cancel();
		}

		private void TakeMeasurement()
		{
			Measure.Custom(sampleGroup, 1f / Time.deltaTime);
		}
	}
}