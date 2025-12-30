using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Fluid.Simulation.Jobs;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(FluidPhysicsGroup))]
	public partial struct FluidPositionInitializationSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<float2> positionBuffer;
		
		public NativeArray<float2> velocityBuffer;

		public int count;


		private EntityQuery fluidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			positionBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			velocityBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			fluidQuery = SystemAPI.QueryBuilder()
				.WithAll<FluidTag>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
				.Build();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			positionBuffer.Dispose();
			velocityBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			count = fluidQuery
				.CalculateEntityCount();

			if (count == 0)
				return;

			var readInitialDataJob = new ReadInitialDataJob
			{
				positions = positionBuffer,
				velocities = velocityBuffer,
			};
			handle = readInitialDataJob.ScheduleParallel(fluidQuery, state.Dependency);
		}
	}
}