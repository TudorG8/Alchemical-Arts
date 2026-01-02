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
	public partial struct ComputeDensitiesJob : IJobEntity 
	{
		[WriteOnly]
		public NativeArray<float> densities;

		[WriteOnly]
		public NativeArray<float> nearDensities;

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
		public FluidSimulationConstantsConfig fluidSimulationConstantsConfig;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			var pos = predictedPositions[index];
			var originCell = SpatialHashingUtility.GetCell2D(pos, spatialPartioningConfig.radius);
			var sqrRadius = spatialPartioningConfig.radius * spatialPartioningConfig.radius;
			var density = 0f;
			var nearDensity = 0f;
			
			foreach (var offset in spatialPartioningConstantsConfig.offsets)
			{
				var hash = SpatialHashingUtility.HashCell2D(originCell + offset);
				var key = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
				var currIndex = spatialOffsets[key];

				while (currIndex < numParticles)
				{
					var neighbourIndex = currIndex;
					var test = spatial[neighbourIndex].index;
					currIndex++;

					var neighbourKey = spatial[neighbourIndex].key;
					if (neighbourKey != key) break;

					var neighbourPos = predictedPositions[test];
					var offsetToNeighbour = neighbourPos - pos;
					var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

					if (sqrDstToNeighbour > sqrRadius) continue;

					var dst = math.sqrt(sqrDstToNeighbour);
					density += SpatialWeightingUtility.ComputeSpikyPow2Weight(fluidSimulationConstantsConfig.spikyPow2ScalingFactor, dst, spatialPartioningConfig.radius);
					nearDensity += SpatialWeightingUtility.ComputeSpikyPow3Weight(fluidSimulationConstantsConfig.spikyPow3ScalingFactor, dst, spatialPartioningConfig.radius);
				}
			}

			densities[index] = density;
			nearDensities[index] = nearDensity;
		}
	}
}