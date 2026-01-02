using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidTag))]
	public partial struct CollectAffectedParticlesJob : IJobEntity
	{
		[WriteOnly]
		public NativeList<int>.ParallelWriter output;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public FluidInputState fluidInputState;


		public void Execute([EntityIndexInQuery] int index)
		{
			var offset = fluidInputState.position - positions[index];
			var sqrDst = math.dot(offset, offset);
			if (sqrDst < fluidInputState.interactionRadius * fluidInputState.interactionRadius)
			{
				output.AddNoResize(index);
			}
		}
	}
}