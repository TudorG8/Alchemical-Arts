using PotionCraft.Core.Fluid.Simulation.Components;
using PotionCraft.Core.Fluid.Simulation.Models;
using PotionCraft.Core.Fluid.Simulation.Utility;
using PotionCraft.Core.Physics.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidTag))]
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
		public SimulationState simulationState;

		[ReadOnly]
		public SimulationConstantsState simulationConstantsState;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			var pos = predictedPositions[index];
			var originCell = SpatialHashingUtility.GetCell2D(pos, simulationState.radius);
			var sqrRadius = simulationState.radius * simulationState.radius;
			var density = 0f;
			var nearDensity = 0f;
			
			foreach (var offset in simulationConstantsState.offsets)
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
					density += SpatialWeightingUtility.ComputeSpikyPow2Weight(simulationConstantsState.spikyPow2ScalingFactor, dst, simulationState.radius);
					nearDensity += SpatialWeightingUtility.ComputeSpikyPow3Weight(simulationConstantsState.spikyPow3ScalingFactor, dst, simulationState.radius);
				}
			}

			densities[index] = density;
			nearDensities[index] = nearDensity;
		}
	}
}