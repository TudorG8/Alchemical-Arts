using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ReadVelocityBatchesJob : IJobEntity
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