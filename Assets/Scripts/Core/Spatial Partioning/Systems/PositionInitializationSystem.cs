using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Groups;
using PotionCraft.Core.SpatialPartioning.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace PotionCraft.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	public partial struct PositionInitializationSystem : ISystem
	{
		public JobHandle handle;


		private EntityQuery fluidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			fluidQuery = SystemAPI.QueryBuilder()
				.WithAll<SimulatedItemTag>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var readInitialDataJob = new ReadInitialDataJob
			{
				positions = fluidBuffersSystem.positionBuffer,
				velocities = fluidBuffersSystem.velocityBuffer,
			};
			handle = readInitialDataJob.ScheduleParallel(fluidQuery, state.Dependency);
		}
	}
}