using Unity.PerformanceTesting;
using UnityEngine;
using static Unity.PerformanceTesting.Measurements.FramesMeasurement;

namespace PotionCraft.Tests.Performance.Utility
{
	public static class PerformanceTestUtility
	{
		public static ScopedFrameTimeMeasurement ScopedFrameTimeWithOrder(string name = "Frame Duration")
		{
			Measure.Custom(name, Time.unscaledDeltaTime * 1000f);
			return new ScopedFrameTimeMeasurement(name);
		}
	}
}