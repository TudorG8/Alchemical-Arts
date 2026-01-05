using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;

namespace AlchemicalArts.Core.Camera.Systems
{
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(SceneSystemGroup))]
	public partial struct SimulationConstantsInitializationSystem : ISystem
	{
		private EntityQuery fluidSimulationConstantsQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			state.RequireForUpdate<FluidSimulationConfig>();
			fluidSimulationConstantsQuery = SystemAPI.QueryBuilder()
				.WithAll<FluidSimulationConstantsConfig>()
				.WithAll<FluidSimulationConstantsConfigInitializeTag>().Build();
			state.RequireForUpdate(fluidSimulationConstantsQuery);
		}
		
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var commandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();
			var fluidSimulationConstantsEntity = fluidSimulationConstantsQuery.GetSingletonEntity();
			var fluidSimulationConstantConfig = SystemAPI.GetComponentRW<FluidSimulationConstantsConfig>(fluidSimulationConstantsEntity);

			fluidSimulationConstantConfig.ValueRW.spikyPow3ScalingFactor = 10 / (math.PI * math.pow(spatialPartioningConfig.radius, 5));
			fluidSimulationConstantConfig.ValueRW.spikyPow2ScalingFactor = 6 / (math.PI * math.pow(spatialPartioningConfig.radius, 4));
			fluidSimulationConstantConfig.ValueRW.spikyPow2DerivativeScalingFactor = 12 / (math.pow(spatialPartioningConfig.radius, 4) * math.PI);
			fluidSimulationConstantConfig.ValueRW.spikyPow3DerivativeScalingFactor = 30 / (math.pow(spatialPartioningConfig.radius, 5) * math.PI);
			fluidSimulationConstantConfig.ValueRW.poly6ScalingFactor = 4 / (math.PI * math.pow(spatialPartioningConfig.radius, 8));
			
			commandBuffer.RemoveComponent<FluidSimulationConstantsConfigInitializeTag>(fluidSimulationConstantsEntity);
		}
	}
}