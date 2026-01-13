using System.Runtime.InteropServices;
using AlchemicalArts.Core.Fluid.Simulation.Jobs;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Shared.Extensions;
using AlchemicalArts.Shared.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
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

	private EntityQuery fluidQuery;


	protected override void OnCreate()
	{
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		simulationStateQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<TemperatureSimulationDisplayConfig>().Build(entityManager);

		RequireForUpdate<PhysicsWorldState>();
		RequireForUpdate(simulationStateQuery);
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
		
		positions = new NativeArray<float2>(10000, Allocator.Persistent);
		temperatures = new NativeArray<float>(10000, Allocator.Persistent);
		positionsBuffer = new ComputeBuffer(10000, Marshal.SizeOf<Vector2>());
		temperatureBuffer = new ComputeBuffer(10000, Marshal.SizeOf<float>());
		material = new Material(fluidSimulationState.shader);
		bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

		material.SetBuffer("Positions", positionsBuffer);
		material.SetBuffer("Temperatures", temperatureBuffer);
		material.SetFloat("Radius", fluidSimulationState.particleSize);
		material.SetFloat("MaxTemperature", fluidSimulationState.temperatureBounds.maximum);
		material.SetTexture("ColourMap", fluidSimulationState.texture);
	}

	protected override void OnUpdate()
	{
		ref var spatialPartioningCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
		ref var temperatureCoordinatorSystem = ref World.Unmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
		if (temperatureCoordinatorSystem.temperatureCount == 0)
			return;
		
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var simulationStateEntity = simulationStateQuery.GetSingletonEntity();
		var simulationState = entityManager.GetComponentData<TemperatureSimulationDisplayConfig>(simulationStateEntity);


		var copyTemperaturePositionJob = new CopyTemperaturePositionJob()
		{
			positionsBufferOutput = positions,
			positionsBufferInput = spatialPartioningCoordinatorSystem.positionBuffer,
		};
		Dependency = copyTemperaturePositionJob.ScheduleParallel(Dependency);
		Dependency.Complete();

		for(int i = 0; i < temperatureCoordinatorSystem.temperatureCount; i++)
		{
			temperatures[i] = math.clamp(temperatureCoordinatorSystem.temperatureStateBuffer[i].temperature, 0, 100);
		}

		positionsBuffer.SetData(positions, 0, 0, temperatureCoordinatorSystem.temperatureCount);
		temperatureBuffer.SetData(temperatures, 0, 0, temperatureCoordinatorSystem.temperatureCount);
		ComputeBufferUtility.CreateArgsBuffer(ref argsBuffer, simulationState.mesh, (uint)temperatureCoordinatorSystem.temperatureCount);
		Graphics.DrawMeshInstancedIndirect(simulationState.mesh, 0, material, bounds, argsBuffer);
	}
}

[BurstCompile]
public partial struct CopyTemperaturePositionJob : IJobEntity
{
	[NativeDisableParallelForRestriction]
	public NativeArray<float2> positionsBufferOutput;

	[ReadOnly]
	public NativeArray<float2> positionsBufferInput;


	public void Execute(
		[EntityIndexInQuery] int i,
		in SpatiallyPartionedIndex spatiallyPartionedItemState,
		in TemperaturePartionedIndex temperaturePartionedIndex)
	{
		positionsBufferOutput[temperaturePartionedIndex.Index] = positionsBufferInput[spatiallyPartionedItemState.index];
	}
}