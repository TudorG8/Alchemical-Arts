using System.Runtime.InteropServices;
using AlchemicalArts.Core.Fluid.Display.Components;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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

		private NativeArray<float2> positions;

		private NativeArray<float2> velocities;

		private Material material;

		private Bounds bounds;

		private EntityQuery fluidQuery;


		protected override void OnCreate()
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<FluidSimulationDisplayConfig>().Build(entityManager);

			RequireForUpdate<PhysicsWorldState>();
			RequireForUpdate(simulationStateQuery);
		}

		protected override void OnDestroy()
		{
			argsBuffer?.Release(); 
			positionsBuffer?.Release();
			velocitiesBuffer?.Release();
			positions.Dispose();
			velocities.Dispose();
		}

		protected override void OnStartRunning()
		{
			var fluidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationState = entityManager.GetComponentData<FluidSimulationDisplayConfig>(fluidSimulationStateEntity);
			
			positions = new NativeArray<float2>(10000, Allocator.Persistent);
			velocities = new NativeArray<float2>(10000, Allocator.Persistent);
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
			ref var spatialPartioningCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var fluidCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<FluidCoordinatorSystem>();
			if (spatialPartioningCoordinatorSystem.count == 0)
				return;
			
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var fluidSimulationState = entityManager.GetComponentData<FluidSimulationDisplayConfig>(fluidSimulationStateEntity);


			var copyFluidPositionJob = new CopyFluidPositionJob()
			{
				positionsBufferOutput = positions,
				positionsBufferInput = spatialPartioningCoordinatorSystem.positionBuffer,
			};
			Dependency = copyFluidPositionJob.ScheduleParallel(Dependency);


			var copyFluidVelocityJob = new CopyFluidVelocityJob()
			{
				velocityBufferOutput = velocities,
				velocityBufferInput = spatialPartioningCoordinatorSystem.velocityBuffer,
			};
			Dependency = copyFluidVelocityJob.ScheduleParallel(Dependency);
			Dependency.Complete();


			positionsBuffer.SetData(positions, 0, 0, fluidCoordinatorSystem.fluidCount);
			velocitiesBuffer.SetData(velocities, 0, 0, fluidCoordinatorSystem.fluidCount);
			ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, fluidSimulationState.mesh, (uint)fluidCoordinatorSystem.fluidCount);
			Graphics.DrawMeshInstancedIndirect(fluidSimulationState.mesh, 0, material, bounds, argsBuffer);
		}
	}
}