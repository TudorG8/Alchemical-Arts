using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;

namespace PotionCraft.Core.Cameras.Systems
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	partial struct SimulationConstantsInitializationSystem : ISystem
	{
		private EntityQuery simulationConstantsQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			simulationConstantsQuery = SystemAPI.QueryBuilder()
				.WithAll<SimulationState>()
				.WithAll<SimulationConstantsState>()
				.WithAll<SimulationConstantsInitializeTag>().Build();
			state.RequireForUpdate(simulationConstantsQuery);
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			var simulationConstantsEntity = simulationConstantsQuery.GetSingletonEntity();
			var simulationConstantsState = SystemAPI.GetComponentRW<SimulationConstantsState>(simulationConstantsEntity);
			var simulationState = SystemAPI.GetComponent<SimulationState>(simulationConstantsEntity);

			for (int y = -1; y <= 1; y++)
			{
				for (int x = -1; x <= 1; x++)
				{
					simulationConstantsState.ValueRW.offsets.Add(new int2(x, y));
				}
			}

			simulationConstantsState.ValueRW.spikyPow3ScalingFactor = 10 / (math.PI * math.pow(simulationState.radius, 5));
			simulationConstantsState.ValueRW.spikyPow2ScalingFactor = 6 / (math.PI * math.pow(simulationState.radius, 4));
			simulationConstantsState.ValueRW.spikyPow2DerivativeScalingFactor = 12 / (math.pow(simulationState.radius, 4) * math.PI);
			simulationConstantsState.ValueRW.spikyPow3DerivativeScalingFactor = 30 / (math.pow(simulationState.radius, 5) * math.PI);
			simulationConstantsState.ValueRW.poly6ScalingFactor = 4 / (math.PI * math.pow(simulationState.radius, 8));
			
			commandBuffer.RemoveComponent<SimulationConstantsInitializeTag>(simulationConstantsEntity);
		}
	}
}