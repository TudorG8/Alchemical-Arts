using PotionCraft.Core.Fluid.Simulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	public partial struct ApplyInwardsForcesJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public NativeList<int> indexes;

		[ReadOnly]
		public FluidInputConfig fluidInputConfig;

		[ReadOnly]
		public FluidInputState fluidInputState;

		[ReadOnly]
		public float deltaTime;


		public void Execute(int index)
		{
			var actualIndex = indexes[index];

			var offset = fluidInputConfig.position - positions[actualIndex];
			var distance = math.length(offset);
			var direction = distance <= float.Epsilon ? float2.zero : offset / distance;
			var smoothingToCenter = math.clamp(distance / fluidInputConfig.interactionRadius, 0, 1);

			var springForce = smoothingToCenter * fluidInputState.interactionStrength * direction;
			var dampingForce = smoothingToCenter * fluidInputState.damping * -velocities[actualIndex];

			velocities[actualIndex] += (springForce + dampingForce) * deltaTime;
		}
	}
}