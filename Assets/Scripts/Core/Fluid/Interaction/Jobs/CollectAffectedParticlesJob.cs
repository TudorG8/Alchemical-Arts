using AlchemicalArts.Core.Fluid.Interaction.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Interaction.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
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