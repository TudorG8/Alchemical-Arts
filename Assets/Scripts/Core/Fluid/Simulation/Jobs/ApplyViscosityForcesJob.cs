using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.SpatialPartioning.Components;
using PotionCraft.Core.SpatialPartioning.Models;
using PotionCraft.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyViscosityForcesJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<SpatialEntry> spatial;
		
		[ReadOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public int numParticles;

		[ReadOnly]
		public SpatialPartioningConfig spatialPartioningConfig;

		[ReadOnly]
		public SpatialPartioningConstantsConfig spatialPartioningConstantsConfig;

		[ReadOnly]
		public FluidSimulationConfig fluidSimulationConfig;

		[ReadOnly]
		public FluidSimulationConstantsConfig fluidSimulationConstantsConfig;

		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int input)
		{
			var pos = predictedPositions[input];
			var originCell = SpatialHashingUtility.GetCell2D(pos, spatialPartioningConfig.radius);
			var sqrRadius = spatialPartioningConfig.radius * spatialPartioningConfig.radius;

			var viscosityForce = float2.zero;
			var velocity = velocities[input];

			foreach (var offset in spatialPartioningConstantsConfig.offsets)
			{
				var hash = SpatialHashingUtility.HashCell2D(originCell + offset);
				var key = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
				var currIndex = spatialOffsets[key];

				while (currIndex < numParticles)
				{
					var spatialNeighbourIndex = currIndex;
					var neighbourIndex = spatial[spatialNeighbourIndex].index;
					currIndex ++;

					if (neighbourIndex == input) continue;
					
					var neighbourKey = spatial[spatialNeighbourIndex].key;
					if (neighbourKey != key) break;

					var neighbourPos = predictedPositions[neighbourIndex];
					var offsetToNeighbour = neighbourPos - pos;
					var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

					if (sqrDstToNeighbour > sqrRadius) continue;

					var dst = math.sqrt(sqrDstToNeighbour);
					var neighbourVelocity = velocities[neighbourIndex];
					viscosityForce += (neighbourVelocity - velocity) * SpatialWeightingUtility.ComputeSmoothingPoly6(fluidSimulationConstantsConfig.poly6ScalingFactor, dst, spatialPartioningConfig.radius);
				}

			}
			velocities[input] += deltaTime * fluidSimulationConfig.viscosityStrength * viscosityForce;
		}
	}
}