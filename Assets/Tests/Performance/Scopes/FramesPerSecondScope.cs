using System;
using System.Threading;
using AlchemicalArts.Shared.Utility;
using Unity.PerformanceTesting;
using UnityEngine;

namespace AlchemicalArts.Tests.Performance.Scopes
{
	public class FramesPerSecondScope : IDisposable
	{
		private readonly CancellationTokenSource cancelSource;

		private readonly SampleGroup sampleGroup;

		private readonly Action takeMeasurement;


		public FramesPerSecondScope(string name = "Frames Per Second")
		{
			cancelSource = new CancellationTokenSource ();
			sampleGroup = new SampleGroup(name, SampleUnit.Undefined);
			takeMeasurement = () => Measure.Custom(sampleGroup, 1f / Time.deltaTime);

			PerformOrderMeasurement();
			TaskUtility.EveryFrame(cancelSource.Token, takeMeasurement);
		}


		public void Dispose()
		{
			cancelSource.Cancel();
		}


		private void PerformOrderMeasurement()
		{
			takeMeasurement();
		}
	}
}