using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(LiquidTag))]
	public partial struct CollectAffectedParticlesJob : IJobEntity
	{
		[WriteOnly]
		public NativeList<int>.ParallelWriter output;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public FluidInputConfig fluidInputConfig;


		public void Execute([EntityIndexInQuery] int index)
		{
			var offset = fluidInputConfig.position - positions[index];
			var sqrDst = math.dot(offset, offset);
			if (sqrDst < fluidInputConfig.interactionRadius * fluidInputConfig.interactionRadius)
			{
				output.AddNoResize(index);
			}
		}
	}
}