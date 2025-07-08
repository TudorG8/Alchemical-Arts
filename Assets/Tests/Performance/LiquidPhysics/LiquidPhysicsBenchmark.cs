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
using PotionCraft.Gameplay.Behaviours;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using PotionCraft.Gameplay.Authoring;
using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

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
			var wrigglerGO = Object.Instantiate(prefab) as GameObject;
			var wriggler = wrigglerGO.GetComponentInChildren<WrigglerBehaviour>();
			var spawnerCount = Object.FindObjectsByType<LiquidSpawnerBehaviour>(FindObjectsSortMode.None).Count();
			var wrigglerRigidBody = wriggler.GetComponent<Rigidbody>();
			wrigglerRigidBody.interpolation = testCase.RigidbodyInterpolation;
			wrigglerRigidBody.collisionDetectionMode = testCase.CollisionDetectionMode;
			wriggler.LimitPerSpawner = Mathf.CeilToInt(4000f / spawnerCount);
			wriggler.SpawnerDelay = 0;
			wriggler.MovementSpeed = 0;
			wriggler.RotationSpeed = 0;
			var liquidRigidbody = wriggler.Liquid.GetComponent<Rigidbody>();
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
			static async Awaitable Test()
			{
				await using (var loadScene = await SceneLoadingScope.Create("LiquidPhysicsBenchmark - Empty Scene"))
				await using (var loadSubscene = await EntitySceneLoadingScope.Create("LiquidPhysicsBenchmark - Subscene Reference"))
				{
					var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
					var wrigglerEntity = new EntityQueryBuilder(Allocator.Temp).WithAll<_WrigglerData>().Build(entityManager).GetSingletonEntity();
					var spawnerCount = new EntityQueryBuilder(Allocator.Temp).WithAll<_LiquidSpawner>().Build(entityManager).CalculateEntityCount();
					var wrigglerData = entityManager.GetComponentData<_WrigglerData>(wrigglerEntity);
					wrigglerData.LimitPerSpawner = Mathf.CeilToInt(4000f / spawnerCount);
					wrigglerData.SpawnerDelay = 0f;
					wrigglerData.MovementSpeed = 0;
					wrigglerData.RotationSpeed = 0;
					entityManager.SetComponentData(wrigglerEntity, wrigglerData);

					await Awaitable.WaitForSecondsAsync(5f);

					using (var fps = new FramesPerSecondScope())
					using (var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder())
					{
						await Awaitable.WaitForSecondsAsync(5f);
					}
				}
			}

			yield return Test();
		}

		public class SceneLoadingScope : IAsyncDisposable
		{
			private readonly Scene scene;


			private SceneLoadingScope(Scene scene)
			{
				this.scene = scene;
			}


			public static async Awaitable<SceneLoadingScope> Create(string scenePath)
			{
				await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

				var scene = SceneManager.GetSceneByName(scenePath);
				SceneManager.SetActiveScene(scene);
				
				return new SceneLoadingScope(scene);
			}

			public async ValueTask DisposeAsync()
			{
				await SceneManager.UnloadSceneAsync(scene);
			}
		}

		public class EntitySceneLoadingScope : IAsyncDisposable
		{
			private readonly Entity sceneEntity;


			private EntitySceneLoadingScope(Entity sceneEntity)
			{
				this.sceneEntity = sceneEntity;
			}


			public static async Awaitable<EntitySceneLoadingScope> Create(string scenePath)
			{
				var entity = await LoadEntitySceneAsync(scenePath);
				return new EntitySceneLoadingScope(entity);
			}

			public async ValueTask DisposeAsync()
			{
				await UnloadEntitySceneAsync(sceneEntity);
			}
		}

		public static async Awaitable<Entity> LoadEntitySceneAsync(string resourceName)
		{
			var world = World.DefaultGameObjectInjectionWorld;
			var subScenePrefab = Resources.Load(resourceName);
			var subSceneGO = Object.Instantiate(subScenePrefab) as GameObject;
			var subSceneComponent = subSceneGO.GetComponent<SubScene>();
			var reference = new EntitySceneReference(subSceneComponent.SceneAsset);

			return await LoadEntitySceneAsync(world.Unmanaged, reference);
		}

		public static async Awaitable<Entity> LoadEntitySceneAsync(WorldUnmanaged world, EntitySceneReference scene)
		{
			var loadedEntity = SceneSystem.LoadSceneAsync(world, scene);
			while (!SceneSystem.IsSceneLoaded(world, loadedEntity))
			{
				await UniTask.NextFrame();
			}
			return loadedEntity;
		}

		public static async Awaitable UnloadEntitySceneAsync(Entity scene)
		{
			await UnloadEntitySceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, scene);
		}

		public static async Awaitable UnloadEntitySceneAsync(WorldUnmanaged world, Entity scene)
		{
			SceneSystem.UnloadScene(world, scene, SceneSystem.UnloadParameters.Default);
			while (SceneSystem.IsSceneLoaded(world, scene))
			{
				await Awaitable.NextFrameAsync();
			}
		}
	}
}