using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidTag))]
	public partial struct WriteVelocityBatchesJob : IJobEntity
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