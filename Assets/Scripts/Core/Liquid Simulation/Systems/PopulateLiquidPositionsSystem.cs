using PotionCraft.Core.LiquidSimulation.Components;
using PotionCraft.Core.LiquidSimulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.LiquidSimulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	partial struct PopulateLiquidPositionsSystem : ISystem
	{
		public NativeArray<float2> positionBuffer;
		
		public NativeArray<float2> velocityBuffer;

		public int count;

		private EntityQuery liquidQuery;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldConfigComponent>();
			positionBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			velocityBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
			liquidQuery = SystemAPI.QueryBuilder()
				.WithAll<LiquidTag>().WithAll<PhysicsBodyConfigComponent>().WithAll<LocalTransform>()
				.Build();
		}

		[BurstCompile]
		public void OnDestroy()
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

			var readJob = new ReadDataJob
			{
				positions = positionBuffer,
				velocities = velocityBuffer,
			};
			var readHandle = readJob.ScheduleParallel(liquidQuery, state.Dependency);
			readHandle.Complete();
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
				in PhysicsBodyConfigComponent body,
				in LocalTransform localTransform)
			{
				positions[index] = localTransform.Position.xy;
				velocities[index] = body.physicsBody.linearVelocity;
			}
		}
	}
}