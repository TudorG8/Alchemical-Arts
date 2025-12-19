using System.Runtime.InteropServices;
using PotionCraft.Core.Fluid.Display.Components;
using PotionCraft.Core.Fluid.Simulation.Systems;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Shared.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Display.Systems
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	partial class LiquidDisplaySystem : SystemBase
	{
		private SystemHandle populateLiquidPositionsSystemHandle;

		private EntityQuery simulationStateQuery;
		
		private ComputeBuffer argsBuffer;

		private ComputeBuffer positionsBuffer;
	
		private ComputeBuffer velocitiesBuffer;

		private Material material;

		private Bounds bounds;


		protected override void OnCreate()
		{
			populateLiquidPositionsSystemHandle = World.Unmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LiquidSimulationConfig>().Build(entityManager);
			RequireForUpdate<PhysicsWorldState>();
			RequireForUpdate(simulationStateQuery);
		}

		protected override void OnDestroy()
		{
			argsBuffer.Release();
			positionsBuffer.Release();
			velocitiesBuffer.Release();
		}

		protected override void OnStartRunning()
		{
			var liquidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var liquidSimulation = entityManager.GetComponentData<LiquidSimulationConfig>(liquidSimulationStateEntity);
			
			positionsBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
			velocitiesBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
			material = new Material(liquidSimulation.shader);
			bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

			material.SetBuffer("Positions", positionsBuffer);
			material.SetBuffer("Velocities", velocitiesBuffer);
			material.SetFloat("Radius", liquidSimulation.particleSize);
			material.SetFloat("MaxVelocity", liquidSimulation.maxVelocity);
			material.SetTexture("ColourMap", liquidSimulation.texture);
		}

		protected override void OnUpdate()
		{
			ref var populateLiquidPositionsSystem = ref World.Unmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var liquidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var liquidSimulation = entityManager.GetComponentData<LiquidSimulationConfig>(liquidSimulationStateEntity);

			if (populateLiquidPositionsSystem.count == 0)
				return;
			
			positionsBuffer.SetData(populateLiquidPositionsSystem.positionBuffer, 0, 0, populateLiquidPositionsSystem.count);
			velocitiesBuffer.SetData(populateLiquidPositionsSystem.velocityBuffer, 0, 0, populateLiquidPositionsSystem.count);
			ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, liquidSimulation.mesh, (uint)populateLiquidPositionsSystem.count);
			Graphics.DrawMeshInstancedIndirect(liquidSimulation.mesh, 0, material, bounds, argsBuffer);
		}
	}
}