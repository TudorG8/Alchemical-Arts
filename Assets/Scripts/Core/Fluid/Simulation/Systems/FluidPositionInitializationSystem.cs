using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	public partial struct FluidPositionInitializationSystem : ISystem
	{
		public JobHandle handle;


		private EntityQuery fluidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			fluidQuery = SystemAPI.QueryBuilder()
				.WithAll<FluidTag>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<FluidBuffersSystem>();
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