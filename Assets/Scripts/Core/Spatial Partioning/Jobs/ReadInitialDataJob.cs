using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.SpatialPartioning.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ReadInitialDataJob : IJobEntity
	{
		[WriteOnly]
		public NativeArray<float2> positions;

		[WriteOnly]
		public NativeArray<float2> velocities;


		public void Execute(
			[EntityIndexInQuery] int index,
			in PhysicsBodyState body,
			in LocalTransform localTransform)
		{
			positions[index] = localTransform.Position.xy;
			velocities[index] = body.physicsBody.linearVelocity;
		}
	}
}