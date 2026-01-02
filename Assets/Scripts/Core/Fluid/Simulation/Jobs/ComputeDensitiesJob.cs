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
		public SimulationConfig simulationConfig;

		[ReadOnly]
		public SimulationConstantsConfig simulationConstantsConfig;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			var pos = predictedPositions[index];
			var originCell = SpatialHashingUtility.GetCell2D(pos, simulationConfig.radius);
			var sqrRadius = simulationConfig.radius * simulationConfig.radius;
			var density = 0f;
			var nearDensity = 0f;
			
			foreach (var offset in simulationConstantsConfig.offsets)
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
					density += SpatialWeightingUtility.ComputeSpikyPow2Weight(simulationConstantsConfig.spikyPow2ScalingFactor, dst, simulationConfig.radius);
					nearDensity += SpatialWeightingUtility.ComputeSpikyPow3Weight(simulationConstantsConfig.spikyPow3ScalingFactor, dst, simulationConfig.radius);
				}
			}

			densities[index] = density;
			nearDensities[index] = nearDensity;
		}
	}
}