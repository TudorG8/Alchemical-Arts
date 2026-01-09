using AlchemicalArts.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AlchemicalArts.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	public partial struct ReadInitialDataJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> positions;

		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedIndex,
			in PhysicsBodyState physicsBodyState,
			in LocalTransform localTransform)
		{
			positions[spatiallyPartionedIndex.index] = localTransform.Position.xy;
			velocities[spatiallyPartionedIndex.index] = physicsBodyState.physicsBody.linearVelocity;
		}
	}
}