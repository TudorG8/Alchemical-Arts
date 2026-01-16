using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Core.SpatialPartioning.Systems;
using AlchemicalArts.Gameplay.Temperature.Components;
using AlchemicalArts.Gameplay.Temperature.Groups;
using AlchemicalArts.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(WriteIndexedNativeArray<TemperaturePartionedIndex, TemperatureState>))]

namespace AlchemicalArts.Gameplay.Temperature.Systems
{
	[UpdateInGroup(typeof(TemperatureGroup), OrderLast = true)]
	partial struct TemperatureWritebackSystem : ISystem
	{
		public JobHandle handle;


		private ComponentTypeHandle<TemperaturePartionedIndex> temperatureIndexTypeHandle;
		
		private ComponentTypeHandle<TemperatureState> temperatureStateTypeHandle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();

			temperatureIndexTypeHandle = state.GetComponentTypeHandle<TemperaturePartionedIndex>(isReadOnly: true);
			temperatureStateTypeHandle = state.GetComponentTypeHandle<TemperatureState>(isReadOnly: false);
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var temperatureCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureCoordinatorSystem>();
			ref var temperatureTransferSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<TemperatureTransferSystem>();
			if (temperatureCoordinatorSystem.temperatureCount == 0)
				return;

			temperatureIndexTypeHandle.Update(ref state);
			temperatureStateTypeHandle.Update(ref state);

			var writeTemperatureStateJob = new WriteIndexedNativeArray<TemperaturePartionedIndex, TemperatureState>()
			{
				buffer = temperatureCoordinatorSystem.temperatureStateBuffer,
				componentHandle = temperatureStateTypeHandle,
				spatialIndexHandle = temperatureIndexTypeHandle,
			};
			handle = writeTemperatureStateJob.ScheduleParallel(temperatureCoordinatorSystem.temperatureQuery, temperatureTransferSystem.handle);
			state.Dependency = handle;
		}
	}
}