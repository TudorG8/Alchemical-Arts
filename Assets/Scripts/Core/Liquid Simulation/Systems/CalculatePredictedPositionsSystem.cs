using PotionCraft.Core.Physics.Components;
using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(LiquidPhysicsGroup))]
[UpdateAfter(typeof(ApplyGravitySystem))]
[UpdateAfter(typeof(InputLiquidForceSystem))]
partial struct CalculatePredictedPositionsSystem : ISystem
{
	public NativeArray<float2> predictedPositionsBuffer;

	private SystemHandle populateLiquidPositionsSystemHandle;

	private float predictionFactor;


	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<PhysicsWorldConfigComponent>();
		predictedPositionsBuffer = new NativeArray<float2>(10000, Allocator.Persistent);
		populateLiquidPositionsSystemHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<PopulateLiquidPositionsSystem>();

		predictionFactor = 1 / 120f;
	}

	[BurstCompile]
	public void OnDestroy()
	{
		predictedPositionsBuffer.Dispose();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		ref var populateLiquidPositionsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<PopulateLiquidPositionsSystem>(populateLiquidPositionsSystemHandle);

		if (populateLiquidPositionsSystem.count == 0)
			return;

		var readJob = new CalculatePredictedPositionsJob
		{
			positions = populateLiquidPositionsSystem.positionBuffer,
			velocities = populateLiquidPositionsSystem.velocityBuffer,
			predictedPositions = predictedPositionsBuffer,
			predictionFactor = predictionFactor
		};
		var readHandle = readJob.ScheduleParallel(state.Dependency);
		readHandle.Complete();
	}

	[BurstCompile]
	public partial struct CalculatePredictedPositionsJob : IJobEntity
	{
		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public NativeArray<float2> velocities;
		
		[ReadOnly]
		public float predictionFactor;
		
		[WriteOnly]
		public NativeArray<float2> predictedPositions;


		void Execute(
			[EntityIndexInQuery] int index,
			in _LiquidTag _)
		{
			predictedPositions[index] = positions[index] + velocities[index] * predictionFactor;
		}
	}
}