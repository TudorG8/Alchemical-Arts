using System.Runtime.InteropServices;
using AlchemicalArts.Core.Fluid.Display.Groups;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Gameplay.Temperature.Components;
using AlchemicalArts.Shared.Extensions;
using AlchemicalArts.Shared.Utility;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(CopySpatialToLocalArrayJob<TemperaturePartionedIndex, float2>))]

namespace AlchemicalArts.Gameplay.Temperature.Systems
{
	[UpdateInGroup(typeof(FluidRenderingGroup))]
	public partial class TemperatureRenderingSystemBase : SystemBase
	{
		private EntityQuery simulationStateQuery;
		
		private ComputeBuffer argsBuffer;

		private ComputeBuffer positionsBuffer;

		private ComputeBuffer temperatureBuffer;

		private NativeArray<float2> positions;

		private NativeArray<float> temperatures;

		private Material material;

		private Bounds bounds;

		private ComponentTypeHandle<SpatiallyPartionedIndex> spatialIndexTypeHandle;

		private ComponentTypeHandle<TemperaturePartionedIndex> temperatureIndexTypeHandle;


		protected override void OnCreate()
		{
			Enabled = false;
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<TemperatureSimulationDisplayConfig>().Build(entityManager);

			RequireForUpdate<PhysicsWorldState>();
			RequireForUpdate(simulationStateQuery);

			positions = new NativeArray<float2>(10000, Allocator.Persistent);
			temperatures = new NativeArray<float>(10000, Allocator.Persistent);
			positionsBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
			temperatureBuffer = new ComputeBuffer(10000, Marshal.SizeOf<float>());
			bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

			temperatureIndexTypeHandle = GetComponentTypeHandle<TemperaturePartionedIndex>(isReadOnly: true);
			spatialIndexTypeHandle = GetComponentTypeHandle<SpatiallyPartionedIndex>(isReadOnly: true);
		}

		protected override void OnDestroy()
		{
			argsBuffer?.Release(); 
			positionsBuffer?.Release();
			temperatureBuffer?.Release();
			positions.Dispose();
			temperatures.Dispose();
		}

		protected override void OnStartRunning()
		{
			var simulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var fluidSimulationState = entityManager.GetComponentData<TemperatureSimulationDisplayConfig>(simulationStateEntity);
			
			material = new Material(fluidSimulationState.shader);

			material.SetBuffer("Positions", positionsBuffer);
			material.SetBuffer("Values", temperatureBuffer);
			material.SetFloat("Radius", fluidSimulationState.particleSize);
			material.SetFloat("MinValue", fluidSimulationState.temperatureBounds.minimum);
			material.SetFloat("MaxValue", fluidSimulationState.temperatureBounds.maximum);
			material.SetTexture("ColourMap", fluidSimulationState.texture);
		}

		protected override void OnUpdate()
		{
			ref var spatialPartioningCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var temperatureCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
			ref var temperatureWritebackSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureWritebackSystem>();
			if (temperatureCoordinatorSystem.temperatureCount == 0)
				return;


			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			var simulationStateEntity = simulationStateQuery.GetSingletonEntity();
			var simulationState = entityManager.GetComponentData<TemperatureSimulationDisplayConfig>(simulationStateEntity);

			temperatureIndexTypeHandle.Update(this);
			spatialIndexTypeHandle.Update(this);

			var copySpatialPositionArrayJob = new CopySpatialToLocalArrayJob<TemperaturePartionedIndex, float2>()
			{
				positionsBufferOutput = positions,
				positionsBufferInput = spatialPartioningCoordinatorSystem.positionBuffer,
				spatialIndexersHandle = spatialIndexTypeHandle,
				indexerHandle = temperatureIndexTypeHandle,
			};
			var copySpatialPositionArrayHandle = copySpatialPositionArrayJob.ScheduleParallel(temperatureCoordinatorSystem.temperatureQuery, temperatureWritebackSystem.handle);
			copySpatialPositionArrayHandle.Complete();

			for(int i = 0; i < temperatureCoordinatorSystem.temperatureCount; i++)
			{
				temperatures[i] = temperatureCoordinatorSystem.temperatureStateBuffer[i].temperature;
			}

			positionsBuffer.SetData(positions, 0, 0, temperatureCoordinatorSystem.temperatureCount);
			temperatureBuffer.SetData(temperatures, 0, 0, temperatureCoordinatorSystem.temperatureCount);
			ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, simulationState.mesh, (uint)temperatureCoordinatorSystem.temperatureCount);
			Graphics.DrawMeshInstancedIndirect(simulationState.mesh, 0, material, bounds, argsBuffer);
		}
	}
}