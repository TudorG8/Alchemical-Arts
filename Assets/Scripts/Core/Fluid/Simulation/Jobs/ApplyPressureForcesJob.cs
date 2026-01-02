
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
	[WithAll(typeof(SimulatedItemTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyPressureForcesJob : IJobEntity
	{
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<SpatialEntry> spatial;

		[ReadOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<float> densities;

		[ReadOnly]
		public NativeArray<float> nearDensity;

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
			[EntityIndexInQuery] int index)
		{
			var density = densities[index];
			var densityNear = nearDensity[index];
			var pressure = PressureFromDensity(density);
			var nearPressure = NearPressureFromDensity(densityNear);
			var pressureForce = new float2();

			var pos = predictedPositions[index];
			var originCell = SpatialHashingUtility.GetCell2D(pos, spatialPartioningConfig.radius);
			var sqrRadius = spatialPartioningConfig.radius * spatialPartioningConfig.radius;
			
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

					if (test == index) continue;

					var neighbourKey = spatial[neighbourIndex].key;
					if (neighbourKey != key) break;

					var neighbourPos = predictedPositions[test];
					var offsetToNeighbour = neighbourPos - pos;
					var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

					if (sqrDstToNeighbour > sqrRadius) continue;

					var dst = math.sqrt(sqrDstToNeighbour);
					var dirToNeighbour = dst > 0 ? offsetToNeighbour / dst : new float2(0, 1);
					
					var neighbourDensity = densities[test];
					var neighbourNearDensity = nearDensity[test];
					var neighbourPressure = PressureFromDensity(neighbourDensity);
					var neighbourNearPressure = NearPressureFromDensity(neighbourNearDensity);
					
					var sharedPressure = (pressure + neighbourPressure) * 0.5f;
					var sharedNearPressure = (nearPressure + neighbourNearPressure) * 0.5f;
					
					pressureForce += sharedPressure * SpatialWeightingUtility.ComputeDerivativeSpikyPow2(fluidSimulationConstantsConfig.spikyPow2DerivativeScalingFactor, dst, spatialPartioningConfig.radius) * dirToNeighbour / neighbourDensity;
					pressureForce += sharedNearPressure * SpatialWeightingUtility.ComputeDerivativeSpikyPow3(fluidSimulationConstantsConfig.spikyPow3DerivativeScalingFactor, dst, spatialPartioningConfig.radius) * dirToNeighbour / neighbourNearDensity;
				}
			}

			var acceleration = pressureForce / density;
			velocities[index] += acceleration * deltaTime;
		}
		
		private readonly float PressureFromDensity(float density)
		{
			return (density - fluidSimulationConfig.targetDensity) * fluidSimulationConfig.pressureMultiplier;
		}

		private readonly float NearPressureFromDensity(float nearDensity)
		{
			return fluidSimulationConfig.nearPressureMultiplier * nearDensity;
		}
	}
}