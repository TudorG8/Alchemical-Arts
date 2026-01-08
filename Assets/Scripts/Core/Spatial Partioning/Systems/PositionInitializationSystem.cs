using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Groups;
using AlchemicalArts.Core.SpatialPartioning.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace AlchemicalArts.Core.SpatialPartioning.Systems
{
	[UpdateInGroup(typeof(SpatialPartioningGroup))]
	public partial struct PositionInitializationSystem : ISystem
	{
		public JobHandle handle;


		private EntityQuery simulatedQuery;

		private EntityQuery fluidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			state.RequireForUpdate<SpatialPartioningConfig>();
			simulatedQuery = SystemAPI.QueryBuilder()
				.WithAll<SpatiallyPartionedItemState>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
			fluidQuery = SystemAPI.QueryBuilder().WithAll<FluidItemTag>().Build();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			ref var fluidBuffersSystem = ref state.WorldUnmanaged.GetUnmanagedSystemRefWithoutHandle<SimulationBuffersSystem>();
			if (fluidBuffersSystem.count == 0)
				return;

			var writeSpatialDataIDJob = new WriteSpatialDataID();
			var writeSpatialDataIDHandle = writeSpatialDataIDJob.Schedule(state.Dependency);

			var writeFluidDataIDJob = new WriteFluidDataID();
			var writeFluidDataIDHandle = writeFluidDataIDJob.Schedule(writeSpatialDataIDHandle);

			var readInitialDataJob = new ReadInitialDataJob
			{
				positionBuffer = fluidBuffersSystem.positionBuffer,
				velocityBuffer = fluidBuffersSystem.velocityBuffer,
			};
			handle = readInitialDataJob.ScheduleParallel(simulatedQuery, writeFluidDataIDHandle);
			handle.Complete();
		}
	}

	[BurstCompile]
	public partial struct WriteSpatialDataID : IJobEntity
	{
		public void Execute(
			[EntityIndexInQuery] int index,
			ref SpatiallyPartionedItemState spatiallyPartionedItemState)
		{
			spatiallyPartionedItemState.index = index;
		}
	}

	[BurstCompile]
	public partial struct WriteFluidDataID : IJobEntity
	{
		public void Execute(
			[EntityIndexInQuery] int index,
			ref FluidItemTag fluidItemState)
		{
			fluidItemState.index = index;
		}
	}
}