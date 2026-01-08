using AlchemicalArts.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct ReadInitialDataJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> positionBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocityBuffer;


		public void Execute(
			in SpatiallyPartionedItemState spatiallyPartionedItemState,
			in PhysicsBodyState body,
			in LocalTransform localTransform)
		{
			positionBuffer[spatiallyPartionedItemState.index] = localTransform.Position.xy;
			velocityBuffer[spatiallyPartionedItemState.index] = body.physicsBody.linearVelocity;
		}
	}
}