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
	[UpdateAfter(typeof(SpatialCoordinatorSystem))]
	public partial struct PositionInitializationSystem : ISystem
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
			if (spatialCoordinatorSystem.count == 0)
				return;

			var readInitialDataJob = new ReadInitialDataJob
			{
				positions = spatialCoordinatorSystem.positionBuffer,
				velocities = spatialCoordinatorSystem.velocityBuffer,
			};
			handle = readInitialDataJob.ScheduleParallel(spatialCoordinatorSystem.simulatedQuery, spatialCoordinatorSystem.handle);
		}
	}
}