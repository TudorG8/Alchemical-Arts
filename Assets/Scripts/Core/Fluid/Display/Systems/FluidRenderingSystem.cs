using System.Runtime.InteropServices;
using AlchemicalArts.Core.Fluid.Display.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Utility;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Display.Systems
{
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public partial class FluidRenderingSystemBase : SystemBase
	{
		private EntityQuery simulationStateQuery;
		
		private ComputeBuffer argsBuffer;

		private ComputeBuffer positionsBuffer;
	
		private ComputeBuffer velocitiesBuffer;

		private Material material;

		private Bounds bounds;


		protected override void OnCreate()
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<FluidSimulationConfig>().Build(entityManager);
			RequireForUpdate<PhysicsWorldState>();
			RequireForUpdate(simulationStateQuery);
		}

		protected override void OnDestroy()
		{
			argsBuffer?.Release(); 
			positionsBuffer?.Release();
			velocitiesBuffer?.Release();
		}

		protected override void OnStartRunning()
		{
			var fluidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationState = entityManager.GetComponentData<FluidSimulationConfig>(fluidSimulationStateEntity);
			
			positionsBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
			velocitiesBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
			material = new Material(fluidSimulationState.shader);
			bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

			material.SetBuffer("Positions", positionsBuffer);
			material.SetBuffer("Velocities", velocitiesBuffer);
			material.SetFloat("Radius", fluidSimulationState.particleSize);
			material.SetFloat("MaxVelocity", fluidSimulationState.maxVelocity);
			material.SetTexture("ColourMap", fluidSimulationState.texture);
		}

		protected override void OnUpdate()
		{
			ref var fluidBuffersSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;
			
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var fluidSimulationState = entityManager.GetComponentData<FluidSimulationConfig>(fluidSimulationStateEntity);

			positionsBuffer.SetData(fluidBuffersSystem.positionBuffer, 0, 0, fluidBuffersSystem.count);
			velocitiesBuffer.SetData(fluidBuffersSystem.velocityBuffer, 0, 0, fluidBuffersSystem.count);
			ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, fluidSimulationState.mesh, (uint)fluidBuffersSystem.count);
			Graphics.DrawMeshInstancedIndirect(fluidSimulationState.mesh, 0, material, bounds, argsBuffer);
		}
	}
}