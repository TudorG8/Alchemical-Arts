using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct PrepareVelocityBatchesJob : IJob
	{
		[ReadOnly]
		public NativeArray<BatchVelocity> batchVelocity;

		[ReadOnly]
		public int count;

		public readonly void Execute()
		{
			unsafe
			{
				var batchVelocityPointer = (BatchVelocity*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(batchVelocity);
				var span = new ReadOnlySpan<BatchVelocity>(batchVelocityPointer, count);
				SetBatchVelocity(span);
			}
		}
	}
}