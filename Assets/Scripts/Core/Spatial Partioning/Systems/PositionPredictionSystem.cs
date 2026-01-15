using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using AlchemicalArts.Shared.Extensions;
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
			ref var spatialCoordinatorSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SpatialCoordinatorSystem>();
			ref var positionInitializationSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<PositionInitializationSystem>();
			if (spatialCoordinatorSystem.count == 0)
				return;

			var spatialPartioningConfig = SystemAPI.GetSingleton<SpatialPartioningConfig>();


			var predictPositionsJob = new PredictPositionsJob
			{
				positions = spatialCoordinatorSystem.positionBuffer,
				velocities = spatialCoordinatorSystem.velocityBuffer,
				predictedPositions = spatialCoordinatorSystem.predictedPositionsBuffer,
				predictionFactor = 1f / spatialPartioningConfig.predictionFrames,
			};
			handle = predictPositionsJob.ScheduleParallel(spatialCoordinatorSystem.simulatedQuery, positionInitializationSystem.handle);
			state.Dependency = handle;
		}
	}
}