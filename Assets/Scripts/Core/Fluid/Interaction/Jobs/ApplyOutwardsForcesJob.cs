using AlchemicalArts.Core.Fluid.Interaction.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Interaction.Jobs
{
	[BurstCompile]
	public partial struct ApplyOutwardsForcesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public FluidInputConfig fluidInputConfig;

		[ReadOnly]
		public FluidInputState fluidInputState;

		[ReadOnly]
		public float deltaTime;


		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedIndex)
		{
			var offset = fluidInputState.position - positions[spatiallyPartionedIndex.index];
			var sqrDst = math.dot(offset, offset);
			if (sqrDst < fluidInputState.interactionRadius * fluidInputState.interactionRadius)
			{
				var distance = math.length(offset);
				var direction = distance <= float.Epsilon ? float2.zero : offset / distance;

				var springForce = -fluidInputConfig.interactionStrength * direction;

				velocities[spatiallyPartionedIndex.index] += springForce * deltaTime;
			}
		}
	}
}