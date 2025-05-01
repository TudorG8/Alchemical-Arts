using System.Collections;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Linq;
using PotionCraft.Tests.Performance.Scopes;
using PotionCraft.Tests.Performance.Utility;

namespace PotionCraft.Tests.Performance
{
	public class LiquidPhysicsBenchmark
	{
		public struct UnityTestCase
		{
			public RigidbodyInterpolation RigidbodyInterpolation { get; set; }

			public CollisionDetectionMode CollisionDetectionMode { get; set; }


			public override readonly string ToString()
				=> $"{RigidbodyInterpolation} - {CollisionDetectionMode}";
		}


		public static UnityTestCase[] TestCases =
		{
			new() { RigidbodyInterpolation = RigidbodyInterpolation.None, CollisionDetectionMode = CollisionDetectionMode.Discrete},
			new() { RigidbodyInterpolation = RigidbodyInterpolation.None, CollisionDetectionMode = CollisionDetectionMode.ContinuousDynamic},
			new() { RigidbodyInterpolation = RigidbodyInterpolation.Extrapolate, CollisionDetectionMode = CollisionDetectionMode.Discrete},
		};


		[UnityTest, Performance]
		public IEnumerator LiquidBouncyness_WithUnity3D_UsingRigidBodySettings([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		{
			yield return SceneManager.LoadSceneAsync("BenchMark");
			var wriggler = UnityEngine.Object.FindFirstObjectByType<WrigglerBehaviour>();
			var spawnerCount = UnityEngine.Object.FindObjectsByType<LiquidSpawnerBehaviour>(FindObjectsSortMode.None).Count();
			var wrigglerRigidBody = wriggler.GetComponent<Rigidbody>();
			wrigglerRigidBody.interpolation = testCase.RigidbodyInterpolation;
			wrigglerRigidBody.collisionDetectionMode = testCase.CollisionDetectionMode;
			wriggler.spawnerDelay = -1;
			wriggler.limitPerSpawner = Mathf.CeilToInt(4000f / spawnerCount);
			wriggler.speed = 100;
			wriggler.rotationSpeed = 45;
			var liquidRigidbody = wriggler.liquid.GetComponent<Rigidbody>();
			liquidRigidbody.interpolation = testCase.RigidbodyInterpolation;
			liquidRigidbody.collisionDetectionMode = testCase.CollisionDetectionMode;

			yield return new WaitForSecondsRealtime(5f);

			using var fps = new FramesPerSecondScope();
			using var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder();

			yield return new WaitForSecondsRealtime(5);
		}
	}
}