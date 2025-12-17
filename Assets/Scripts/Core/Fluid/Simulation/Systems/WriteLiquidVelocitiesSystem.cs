using System;
using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Groups;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace PotionCraft.Core.Fluid.Simulation.Systems
{
	[UpdateInGroup(typeof(LiquidPhysicsGroup))]
	[UpdateAfter(typeof(ViscositySystem))]
	partial struct WriteLiquidVelocitiesSystem : ISystem
	{
		private SystemHandle populateLiquidPositionsSystemHandle;

		private NativeArray<BatchVelocity> batchVelocityBuffer;


		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsWorldState>();
			batchVelocityBuffer = new NativeArray<BatchVelocity>(10000, Allocator.Persistent);
			populateLiquidPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();
		}

		[BurstCompile]
		public void OnDestroy(ref SystemState state)
		{
			batchVelocityBuffer.Dispose();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state) 
		{
			ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);

			if (populateLiquidPositionsSystem.count == 0)
				return;

			var writeParticlesJob = new WriteParticlesJob
			{
				batchVelocityBuffer = batchVelocityBuffer,
				velocityBuffer = populateLiquidPositionsSystem.velocityBuffer,
			};
			var writeParticlesHandle = writeParticlesJob.ScheduleParallel(state.Dependency);
			writeParticlesHandle.Complete();
			unsafe
			{
				var batchVelocityPointer = (BatchVelocity*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(batchVelocityBuffer);
				var span = new ReadOnlySpan<BatchVelocity>(batchVelocityPointer, populateLiquidPositionsSystem.count);
				SetBatchVelocity(span);
			}
		}

		[BurstCompile]
		[WithAll(typeof(LiquidTag))]
		public partial struct WriteParticlesJob : IJobEntity
		{
			[ReadOnly]
			public NativeArray<float2> velocityBuffer;

			[WriteOnly]
			public NativeArray<BatchVelocity> batchVelocityBuffer;


			void Execute(
				[EntityIndexInQuery] int index,
				ref PhysicsBodyState body)
			{
				batchVelocityBuffer[index] = new BatchVelocity
				{
					physicsBody = body.physicsBody,
					linearVelocity = velocityBuffer[index]
				};
			}
		}
	}
}