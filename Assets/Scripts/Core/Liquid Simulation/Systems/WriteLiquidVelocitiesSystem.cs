using System;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

[UpdateInGroup(typeof(LiquidPhysicsGroup))]
[UpdateAfter(typeof(SpatialDataSystem))]
partial struct WriteLiquidVelocitiesSystem : ISystem
{
	private SystemHandle populateLiquidPositionsSystemHandle;

	private NativeArray<BatchVelocity> batchVelocityBuffer;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldConfigComponent>();
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
	public partial struct WriteParticlesJob : IJobEntity
	{
		[ReadOnly]
		public NativeArray<float2> velocityBuffer;

		[WriteOnly]
		public NativeArray<BatchVelocity> batchVelocityBuffer;


		void Execute(
			[EntityIndexInQuery] int index,
			in _LiquidTag _,
			ref PhysicsBodyConfigComponent body)
		{
			batchVelocityBuffer[index] = new BatchVelocity
			{
				physicsBody = body.physicsBody,
				linearVelocity = velocityBuffer[index]
			};
		}
	}
}
