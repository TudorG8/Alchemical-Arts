using UnityEngine;
using Unity.Entities;
using UnityEngine.TestTools;
using Unity.PerformanceTesting;
using System.Collections;
using Unity.Collections;
using AlchemicalArts.Tests.Performance.Utility;
using AlchemicalArts.Gameplay.Prototype.Components;
using AlchemicalArts.Tests.Performance.Scopes;
using NUnit.Framework;
using AlchemicalArts.Shared.Scopes;

namespace AlchemicalArts.Tests.Performance
{
	public class PhysicsEngineBenchmark
	{
		public struct UnityTestCase
		{
			public int LiquidCount { get; set; }


			public override readonly string ToString()
				=> $"With {LiquidCount} Particles";
		}

		public static UnityTestCase[] TestCases =
		{
			new() { LiquidCount = 2000 },
			new() { LiquidCount = 5000 },
			new() { LiquidCount = 10000 },
		};


		[UnityTest, Performance]
		public IEnumerator PhysicsPerformance_WithBox2D([ValueSource(nameof(TestCases))] UnityTestCase testCase)
		{
			async Awaitable Test()
			{
				await using (var loadScene = await SceneLoadingScope.Create("PhysicsEngineBenchmark - Box2D - Scene"))
				{
					await Awaitable.WaitForSecondsAsync(0.5f);

					var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
					var fluidSpawnerConfigQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<FluidSpawnerConfig>().Build(entityManager);
					var fluidSpawnerConfigs = fluidSpawnerConfigQuery.ToComponentDataArray<FluidSpawnerConfig>(Allocator.Temp);
					var fluidEntities = fluidSpawnerConfigQuery.ToEntityArray(Allocator.Temp);
					var count = fluidSpawnerConfigQuery.CalculateEntityCount();
					for(int i = 0; i < count; i++)
					{
						var fluidSpawnerConfig = fluidSpawnerConfigs[i];
						fluidSpawnerConfig.max = Mathf.FloorToInt(testCase.LiquidCount / count);
						fluidSpawnerConfigs[i] = fluidSpawnerConfig;
					}
					fluidSpawnerConfigQuery.CopyFromComponentDataArray(fluidSpawnerConfigs);

					// Wait for fluid to settle
					await Awaitable.WaitForSecondsAsync(5f);

					using (var fps = new FramesPerSecondScope())
					using (var frameTime = PerformanceTestUtility.ScopedFrameTimeWithOrder())
					{
						// Start checking performance
						await Awaitable.WaitForSecondsAsync(5f);
					}
				}
			}

			yield return Test();
		}
	}
}