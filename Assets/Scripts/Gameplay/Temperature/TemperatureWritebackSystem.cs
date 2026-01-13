using AlchemicalArts.Core.Fluid.Simulation.Systems;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(TemperatureTransferSystem))]
partial struct TemperatureWritebackSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldState>();
		state.RequireForUpdate<SpatialPartioningConfig>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		ref var temperatureCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
		ref var temperatureTransferSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureTransferSystem>();
		if (temperatureCoordinatorSystem.temperatureCount == 0)
			return;

		var writeTemperatureStateJob = new WriteTemperatureStateJob()
		{
			temperatureStateBuffer = temperatureCoordinatorSystem.temperatureStateBuffer,
		};
		var handle = writeTemperatureStateJob.ScheduleParallel(temperatureTransferSystem.handle);
		handle.Complete();
	}
}

[BurstCompile]
public partial struct WriteTemperatureStateJob : IJobEntity
{
	[NativeDisableParallelForRestriction]
	public NativeArray<TemperatureState> temperatureStateBuffer;
	
	public void Execute(
		[EntityIndexInQuery] int index,
		ref TemperatureState temperatureState,
		in TemperaturePartionedIndex temperaturePartionedIndex)
	{
		temperatureState = temperatureStateBuffer[temperaturePartionedIndex.Index];
	}
}