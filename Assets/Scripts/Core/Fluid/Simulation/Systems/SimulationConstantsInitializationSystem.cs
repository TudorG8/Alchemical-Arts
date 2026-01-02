using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;

namespace PotionCraft.Core.Camera.Systems
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	public partial struct SimulationConstantsInitializationSystem : ISystem
	{
		private EntityQuery simulationConstantsQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			simulationConstantsQuery = SystemAPI.QueryBuilder()
				.WithAll<SimulationConfig>()
				.WithAll<SimulationConstantsConfig>()
				.WithAll<SimulationConstantsConfigInitializeTag>().Build();
			state.RequireForUpdate(simulationConstantsQuery);
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			var simulationConstantsEntity = simulationConstantsQuery.GetSingletonEntity();
			var simulationConstantsConfig = SystemAPI.GetComponentRW<SimulationConstantsConfig>(simulationConstantsEntity);
			var simulationConfig = SystemAPI.GetComponent<SimulationConfig>(simulationConstantsEntity);

			for (int y = -1; y <= 1; y++)
			{
				for (int x = -1; x <= 1; x++)
				{
					simulationConstantsConfig.ValueRW.offsets.Add(new int2(x, y));
				}
			}

			simulationConstantsConfig.ValueRW.spikyPow3ScalingFactor = 10 / (math.PI * math.pow(simulationConfig.radius, 5));
			simulationConstantsConfig.ValueRW.spikyPow2ScalingFactor = 6 / (math.PI * math.pow(simulationConfig.radius, 4));
			simulationConstantsConfig.ValueRW.spikyPow2DerivativeScalingFactor = 12 / (math.pow(simulationConfig.radius, 4) * math.PI);
			simulationConstantsConfig.ValueRW.spikyPow3DerivativeScalingFactor = 30 / (math.pow(simulationConfig.radius, 5) * math.PI);
			simulationConstantsConfig.ValueRW.poly6ScalingFactor = 4 / (math.PI * math.pow(simulationConfig.radius, 8));
			
			commandBuffer.RemoveComponent<SimulationConstantsConfigInitializeTag>(simulationConstantsEntity);
		}
	}
}