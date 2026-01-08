using AlchemicalArts.Core.Fluid.Simulation.Components;
using AlchemicalArts.Core.Physics.Components;
using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Models;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Core.Fluid.Simulation.Jobs
{
	[BurstCompile]
	[WithAll(typeof(FluidItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ComputeDensitiesJob : IJobEntity 
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<float> densities;

		[NativeDisableParallelForRestriction]
		public NativeArray<float> nearDensities;

		[ReadOnly]
		public NativeArray<FluidSpatialEntry> spatial;
		
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
			[EntityIndexInQuery] int i,
			in SpatiallyPartionedItemState spatiallyPartionedItemState,
			in FluidItemTag fluidItemTag)
		{
			// var fluidIndex = spatial[i].fluidIndex;
			// var simulationIndex = spatial[i].simulationIndex;
			var fluidIndex = fluidItemTag.index;
			var simulationIndex = spatiallyPartionedItemState.index;
			
			var position = predictedPositions[simulationIndex];
			var originCell = SpatialHashingUtility.GetCell2D(position, spatialPartioningConfig.radius);
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
					var neighbourSimulationIndex = spatial[neighbourIndex].simulationIndex;
					currIndex++;

					var neighbourKey = spatial[neighbourIndex].key;
					if (neighbourKey != key) break;

					var neighbourPos = predictedPositions[neighbourSimulationIndex];
					var offsetToNeighbour = neighbourPos - position;
					var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

					if (sqrDstToNeighbour > sqrRadius) continue;

					var dst = math.sqrt(sqrDstToNeighbour);
					density += SpatialWeightingUtility.ComputeSpikyPow2Weight(fluidSimulationConstantsConfig.spikyPow2ScalingFactor, dst, spatialPartioningConfig.radius);
					nearDensity += SpatialWeightingUtility.ComputeSpikyPow3Weight(fluidSimulationConstantsConfig.spikyPow3ScalingFactor, dst, spatialPartioningConfig.radius);
				}
			}

			densities[fluidIndex] = density;
			nearDensities[fluidIndex] = nearDensity;
		}
	}
}