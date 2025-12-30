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
using Cysharp.Threading.Tasks;
using Unity.Collections;
using PotionCraft.Gameplay.Authoring;
using System;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace PotionCraft.Tests.Performance
{
	public class FluidPhysicsBenchmark
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


		// [UnityTest, Performance]
		// public IEnumerator FluidBouncyness_WithDOTS_UsingRigidBodySettings([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		// {
		// 	static async Awaitable Test()
		// 	{
		// 		await using (var loadScene = await SceneLoadingScope.Create("FluidPhysicsBenchmark - Empty Scene"))
		// 		await using (var loadSubscene = await EntitySceneLoadingScope.Create("DOTS/FluidPhysicsBenchmark - DOTS - Subscene Reference"))
		// 		{
		// 			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		// 			var wrigglerEntity = new EntityQueryBuilder(Allocator.Temp).WithAll<_WrigglerData>().Build(entityManager).GetSingletonEntity();
		// 			var spawnerCount = new EntityQueryBuilder(Allocator.Temp).WithAll<_FluidSpawner>().Build(entityManager).CalculateEntityCount();
		// 			var wrigglerData = entityManager.GetComponentData<_WrigglerData>(wrigglerEntity);
		// 			wrigglerData.LimitPerSpawner = Mathf.CeilToInt(10000f / spawnerCount);
		// 			wrigglerData.SpawnerDelay = 0f;
		// 			wrigglerData.MovementSpeed = 0;
		// 			wrigglerData.RotationSpeed = 0;
		// 			entityManager.SetComponentData(wrigglerEntity, wrigglerData);

		// 			await Awaitable.WaitForSecondsAsync(5f);

		// 			using (var fps = new FramesPerSecondScope())
		// 			using (var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder())
		// 			{
		// 				await Awaitable.WaitForSecondsAsync(30f);
		// 			}
		// 		}
		// 	}

		// 	yield return Test();
		// }

		// [UnityTest, Performance]
		// public IEnumerator FluidBouncyness_WithBox2D_UsingRigidBodySettings([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		// {
		// 	static async Awaitable Test()
		// 	{
		// 		await using (var loadScene = await SceneLoadingScope.Create("FluidPhysicsBenchmark - Empty Scene"))
		// 		await using (var loadSubscene = await EntitySceneLoadingScope.Create("Box2D/FluidPhysicsBenchmark - Box2D - Subscene Reference"))
		// 		{
		// 			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		// 			var wrigglerEntity = new EntityQueryBuilder(Allocator.Temp).WithAll<_WrigglerData>().Build(entityManager).GetSingletonEntity();
		// 			var spawnerCount = new EntityQueryBuilder(Allocator.Temp).WithAll<_FluidSpawner>().Build(entityManager).CalculateEntityCount();
		// 			var wrigglerData = entityManager.GetComponentData<_WrigglerData>(wrigglerEntity);
		// 			wrigglerData.LimitPerSpawner = Mathf.CeilToInt(10000f / spawnerCount);
		// 			wrigglerData.SpawnerDelay = 0f;
		// 			wrigglerData.MovementSpeed = 0;
		// 			wrigglerData.RotationSpeed = 0;
		// 			entityManager.SetComponentData(wrigglerEntity, wrigglerData);

		// 			await Awaitable.WaitForSecondsAsync(5f);

		// 			using (var fps = new FramesPerSecondScope())
		// 			using (var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder())
		// 			{
		// 				await Awaitable.WaitForSecondsAsync(30f);
		// 			}
		// 		}
		// 	}

		// 	yield return Test();
		// }

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