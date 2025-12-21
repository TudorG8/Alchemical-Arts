using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	public partial struct PopulateLiquidPositionsSystem : ISystem
	{
		public JobHandle handle;

		public NativeArray<float2> positionBuffer;
		
		public NativeArray<float2> velocityBuffer;

		public int count;


		private EntityQuery liquidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			positionBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			velocityBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			liquidQuery = SystemAPI.QueryBuilder()
				.WithAll<LiquidTag>().WithAll<PhysicsBodyState>().WithAll<LocalTransform>()
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
			count = liquidQuery
				.CalculateEntityCount();

			if (count == 0)
				return;

			var readDataJob = new ReadDataJob
			{
				positions = positionBuffer,
				velocities = velocityBuffer,
			};
			handle = readDataJob.ScheduleParallel(liquidQuery, state.Dependency);
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		public partial struct ReadDataJob : IJobEntity
		{
			[WriteOnly]
			public NativeArray<float2> positions;

			[WriteOnly]
			public NativeArray<float2> velocities;


			void Execute(
				[EntityIndexInQuery] int index,
				in PhysicsBodyState body,
				in LocalTransform localTransform)
			{
				positions[index] = localTransform.Position.xy;
				velocities[index] = body.physicsBody.linearVelocity;
			}
		}
	}
}