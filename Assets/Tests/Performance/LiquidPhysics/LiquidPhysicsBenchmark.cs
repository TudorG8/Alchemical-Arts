using System.Collections;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using System.Linq;
using PotionCraft.Tests.Performance.Scopes;
using PotionCraft.Tests.Performance.Utility;
using Unity.Entities;
using Unity.Scenes;
using Unity.Entities.Serialization;
using UnityEditor;
using PotionCraft.Gameplay.Behaviours;

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
			new() { RigidbodyInterpolation = RigidbodyInterpolation.Extrapolate, CollisionDetectionMode = CollisionDetectionMode.Discrete},
		};


		[UnityTest, Performance]
		public IEnumerator LiquidBouncyness_WithUnity3D_UsingRigidBodySettings([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		{
			yield return SceneManager.LoadSceneAsync("LiquidPhysicsBenchmark - Empty Scene");
			var prefab = Resources.Load("LiquidPhysicsBenchmark - Unity Wriggler");
			var obj = GameObject.Instantiate(prefab);
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

		[UnityTest, Performance]
		public IEnumerator LiquidBouncyness_WithDOTS_UsingRigidBodySettings([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		{
			yield return SceneManager.LoadSceneAsync("LiquidPhysicsBenchmark - Empty Scene");
	
			var world = World.DefaultGameObjectInjectionWorld;
			
			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
				"Assets/Tests/Performance/LiquidPhysics/Resources/LiquidPhysicsBenchmark - Empty Subscene.unity");
			var reference = new EntitySceneReference(sceneAsset);
			yield return LoadEntitySceneAsync(world.Unmanaged, reference);

			sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
				"Assets/Tests/Performance/LiquidPhysics/Resources/LiquidPhysicsBenchmark - Baked Prefabs.unity");
			reference = new EntitySceneReference(sceneAsset);
			yield return LoadEntitySceneAsync(world.Unmanaged, reference);

			yield return new WaitForSecondsRealtime(5f);

			using var fps = new FramesPerSecondScope();
			using var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder();

			yield return new WaitForSecondsRealtime(5f);
		}

		public static async Awaitable LoadEntitySceneAsync(WorldUnmanaged world, EntitySceneReference scene)
		{
			Entity handle = SceneSystem.LoadSceneAsync(world, scene);
			while (!SceneSystem.IsSceneLoaded(world, handle))
			{
				await Awaitable.NextFrameAsync();
			}
		}
	}
}