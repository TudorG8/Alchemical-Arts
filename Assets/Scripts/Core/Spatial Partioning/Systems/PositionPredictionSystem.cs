using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	[UpdateAfter(typeof(PositionInitializationSystem))]
	public partial struct PositionPredictionSystem : ISystem
	{
		public JobHandle handle;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			ref var fluidPositionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionInitializationSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var simulationConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();

			fluidPositionInitializationSystem.handle.Complete();

			var predictPositionsJob = new PredictPositionsJob
			{
				positionBuffer = fluidBuffersSystem.positionBuffer,
				velocityBuffer = fluidBuffersSystem.velocityBuffer,
				predictedPositionsBuffer = fluidBuffersSystem.predictedPositionsBuffer,
				predictionFactor = 1f / simulationConfig.predictionFrames,
			};
			handle = predictPositionsJob.Schedule(fluidPositionInitializationSystem.handle);
			handle.Complete();
		}
	}
}