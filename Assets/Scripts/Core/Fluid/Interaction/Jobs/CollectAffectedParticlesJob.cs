using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Interaction.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidPartionedIndex))]
	public partial struct CollectAffectedParticlesJob : IJobEntity
	{
		[WriteOnly]
		public NativeList<int>.ParallelWriter output;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public FluidInputState fluidInputState;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedIndex)
		{
			var offset = fluidInputState.position - positions[spatiallyPartionedIndex.index];
			var sqrDst = math.dot(offset, offset);
			if (sqrDst < fluidInputState.interactionRadius * fluidInputState.interactionRadius)
			{
				output.AddNoResize(spatiallyPartionedIndex.index);
			}
		}
	}
}