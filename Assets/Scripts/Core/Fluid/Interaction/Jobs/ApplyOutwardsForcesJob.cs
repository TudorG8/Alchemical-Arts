using PotionCraft.Core.Fluid.Interaction.Components;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Interaction.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyOutwardsForcesJob : IJobEntity
	{
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<float2> positions;

		[ReadOnly]
		public FluidInputConfig fluidInputConfig;

		[ReadOnly]
		public FluidInputState fluidInputState;

		[ReadOnly]
		public float deltaTime;


		public void Execute([EntityIndexInQuery] int index)
		{
			var offset = fluidInputState.position - positions[index];
			var sqrDst = math.dot(offset, offset);
			if (sqrDst < fluidInputState.interactionRadius * fluidInputState.interactionRadius)
			{
				var distance = math.length(offset);
				var direction = distance <= float.Epsilon ? float2.zero : offset / distance;

				var springForce = -fluidInputConfig.interactionStrength * direction;

				velocities[index] += springForce * deltaTime;
			}
		}
	}
}