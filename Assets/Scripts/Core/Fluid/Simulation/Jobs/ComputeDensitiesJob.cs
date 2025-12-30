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
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ComputeDensitiesJob : IJobEntity 
	{
		[WriteOnly]
		public NativeArray<float> densities;

		[WriteOnly]
		public NativeArray<float> nearDensities;

		[ReadOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<SpatialEntry> spatial;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public float smoothingRadius;

		[ReadOnly]
		public int numParticles;

		[ReadOnly]
		public NativeArray<int2> offsets2D;

		[ReadOnly]
		public float spikyPow3ScalingFactor;

		[ReadOnly]
		public float spikyPow2ScalingFactor;

		[ReadOnly]
		public int hashingLimit;


		public void Execute(
			[EntityIndexInQuery] int index)
		{
			var pos = predictedPositions[index];
			var originCell = SpatialHashingUtility.GetCell2D(pos, smoothingRadius);
			var sqrRadius = smoothingRadius * smoothingRadius;
			var density = 0f;
			var nearDensity = 0f;
			
			for(int i = 0; i < 9; i++)
			{
				var hash = SpatialHashingUtility.HashCell2D(originCell + offsets2D[i]);
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
					density += SpatialWeightingUtility.ComputeSpikyPow2Weight(spikyPow2ScalingFactor, dst, smoothingRadius);
					nearDensity += SpatialWeightingUtility.ComputeSpikyPow3Weight(spikyPow3ScalingFactor, dst, smoothingRadius);
				}
			}

			densities[index] = density;
			nearDensities[index] = nearDensity;
		}
	}
}