using Unity.Entities;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;
using System.Collections;
using NUnit.Framework;
using AlchemicalArts.Shared.Extensions;
using UnityEngine;
using Unity.Transforms;
using UnityEngine.SceneManagement;
using System;
using AlchemicalArts.Tests.Performance.Shared.Executors;
using AlchemicalArts.Tests.Performance.Shared.Constants;
using AlchemicalArts.Tests.Performance.Shared.Scopes;
using AlchemicalArts.Tests.Performance.Shared.Utility;

namespace AlchemicalArts.Tests.Performance
{
	public class PhysicsEngineBenchmark
	{
		public struct UnityTestCase
		{
			public string SceneName { get; set; }
			public int LiquidCount { get; set; }


			public override readonly string ToString()
				=> $"Scene: {SceneName}, Count: {LiquidCount}";
		}

		public static UnityTestCase[] TestCases =
		{
			new() { SceneName = BOX2D_SCENE_NAME, LiquidCount = 2000 },
			new() { SceneName = BOX2D_SCENE_NAME, LiquidCount = 5000 },
			new() { SceneName = BOX2D_SCENE_NAME, LiquidCount = 10000 },
			new() { SceneName = DOTS_SCENE_NAME, LiquidCount = 2000 },
			new() { SceneName = DOTS_SCENE_NAME, LiquidCount = 5000 },
			new() { SceneName = DOTS_SCENE_NAME, LiquidCount = 10000 },
		};

		private const string BOX2D_SCENE_NAME = "PhysicsEngineBenchmark - Box2D - Scene";

		private const string DOTS_SCENE_NAME = "PhysicsEngineBenchmark - DOTS - Scene";


		[UnityTest, Performance]
		public IEnumerator PhysicsPerformance_OfStackingObjects_With([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		{
			async Awaitable Test()
			{
				var world = World.DefaultGameObjectInjectionWorld;
				
				await SceneManager.LoadSceneAsync(testCase.SceneName, LoadSceneMode.Single);
				var physicsEngineTestData = await world.EntityManager.GetComponentDataAsync<PhysicsEngineTestData>();

				var random = new Unity.Mathematics.Random((uint)DateTime.UtcNow.Ticks);
				// We use endfixedstep here as parenting needs to happen after physics but before transform
				var commandBuffer = world.GetExistingSystemManaged<EndFixedStepSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
				var boundedActionInvoker = new BoundedGridExecutioner((position) => Spawn(ref random, commandBuffer, physicsEngineTestData, position), physicsEngineTestData.position, physicsEngineTestData.bounds, testCase.LiquidCount);
				boundedActionInvoker.Execute();

				await Awaitable.WaitForSecondsAsync(PerformanceTestConstants.SETTLE_PERIOD_TIMER);

				using (var fps = new FramesPerSecondScope())
				using (var frameTime = PerformanceTestUtility.NamedFrameTimeScope())
				{
					await Awaitable.WaitForSecondsAsync(PerformanceTestConstants.PERFORMANCE_CHECKING_TIMER);
				}
			}


			yield return Test();
		}


		private void Spawn(ref Unity.Mathematics.Random random, EntityCommandBuffer ecb, PhysicsEngineTestData testData, Vector2 position)
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			
			var localTransform = entityManager.GetComponentData<LocalTransform>(testData.testObject);
			localTransform.Position = position.ToFloat3() + random.NextFloat2Direction().ToFloat3() * 0.1f;
			
			var newEntity = ecb.Instantiate(testData.testObject);
			ecb.SetComponent(newEntity, localTransform);
			ecb.AddComponent(newEntity, new Parent() { Value = testData.folder });
		}
	}
}