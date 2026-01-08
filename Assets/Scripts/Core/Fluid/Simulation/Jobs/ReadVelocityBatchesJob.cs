using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ReadVelocityBatchesJob : IJobEntity
	{
		[ReadOnly]
		public NativeArray<FluidSpatialEntry> spatial;

		[ReadOnly]
		public NativeArray<float2> velocityBuffer;

		[NativeDisableParallelForRestriction]
		public NativeArray<BatchVelocity> batchVelocityBuffer;


		void Execute(
			in SpatiallyPartionedItemState spatiallyPartionedItemState,
			in PhysicsBodyState body,
			in LocalTransform localTransform)
		{
			batchVelocityBuffer[spatiallyPartionedItemState.index] = new BatchVelocity
			{
				physicsBody = body.physicsBody,
				linearVelocity = velocityBuffer[spatiallyPartionedItemState.index]
			};
		}
	}
}