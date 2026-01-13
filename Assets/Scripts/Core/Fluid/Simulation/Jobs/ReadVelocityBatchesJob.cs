using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct ReadVelocityBatchesJob : IJobEntity
	{
		[ReadOnly]
		public NativeArray<float2> velocityBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<BatchVelocity> batchVelocityBuffer;


		void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedItemState,
			in PhysicsBodyState body)
		{
			batchVelocityBuffer[spatiallyPartionedItemState.index] = new BatchVelocity
			{
				physicsBody = body.physicsBody,
				linearVelocity = velocityBuffer[spatiallyPartionedItemState.index]
			};
		}
	}
}