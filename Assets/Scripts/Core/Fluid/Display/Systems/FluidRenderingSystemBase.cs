using System.Runtime.InteropServices;
using AlchemicalArts.Core.Fluid.Display.Components;
using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using AlchemicalArts.Shared.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(CopySpatialToLocalArrayJob<FluidPartionedIndex, float2>))]

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

		private ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexTypeHandle;

		private ComponentTypeHandle<FluidPartionedIndex> fluidIndexTypeHandle;


		protected override void OnCreate()
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<FluidSimulationDisplayConfig>().Build(entityManager);

			RequireForUpdate<PhysicsWorldState>();
			RequireForUpdate(simulationStateQuery);

			fluidIndexTypeHandle = GetComponentTypeHandle<FluidPartionedIndex>(isReadOnly: true);
			spatialIndexTypeHandle = GetComponentTypeHandle<SpatiallyPartionedIndex>(isReadOnly: true);
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
			if (fluidCoordinatorSystem.fluidCount == 0)
				return;
			
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var fluidSimulationState = entityManager.GetComponentData<FluidSimulationDisplayConfig>(fluidSimulationStateEntity);

			fluidIndexTypeHandle.Update(this);
			spatialIndexTypeHandle.Update(this);

			var copyFluidPositionJob = new CopySpatialToLocalArrayJob<FluidPartionedIndex, float2>()
			{
				positionsBufferOutput = positions,
				positionsBufferInput = spatialPartioningCoordinatorSystem.positionBuffer,
				spatialIndexersHandle = spatialIndexTypeHandle,
				indexerHandle = fluidIndexTypeHandle,
			};
			var copyFluidPositionHandle = copyFluidPositionJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, Dependency);

			var copyFluidVelocityJob = new CopySpatialToLocalArrayJob<FluidPartionedIndex, float2>()
			{
				positionsBufferOutput = velocities,
				positionsBufferInput = spatialPartioningCoordinatorSystem.velocityBuffer,
				spatialIndexersHandle = spatialIndexTypeHandle,
				indexerHandle = fluidIndexTypeHandle,
			};
			var copyFluidVelocityHandle = copyFluidVelocityJob.ScheduleParallel(fluidCoordinatorSystem.fluidQuery, copyFluidPositionHandle);

			var handle = JobHandle.CombineDependencies(copyFluidPositionHandle, copyFluidVelocityHandle);
			Dependency = handle;
			handle.Complete();

			positionsBuffer.SetData(positions, 0, 0, fluidCoordinatorSystem.fluidCount);
			velocitiesBuffer.SetData(velocities, 0, 0, fluidCoordinatorSystem.fluidCount);
			ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, fluidSimulationState.mesh, (uint)fluidCoordinatorSystem.fluidCount);
			Graphics.DrawMeshInstancedIndirect(fluidSimulationState.mesh, 0, material, bounds, argsBuffer);
		}
	}
}