using Unity.PerformanceTesting;
using UnityEngine;
using static Unity.PerformanceTesting.Measurements.FramesMeasurement;

namespace AlchemicalArts.Tests.Performance.Shared.Utility
{
	public static class PerformanceTestUtility
	{
		public static ScopedFrameTimeMeasurement NamedFrameTimeScope(string name = "Frame Duration")
		{
			Measure.Custom(name, Time.unscaledDeltaTime * 1000f);
			return new ScopedFrameTimeMeasurement(name);
		}
	}
}