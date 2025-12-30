
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
	[WithAll(typeof(LiquidTag))]
	[WithAll(typeof(PhysicsBodyState))]
	public partial struct ApplyPressureForcesJob : IJobEntity
	{
		public NativeArray<float2> velocities;

		[ReadOnly]
		public NativeArray<float> densities;

		[ReadOnly]
		public NativeArray<float> nearDensity;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<SpatialEntry> spatial;

		[ReadOnly]
		public float smoothingRadius;

		[ReadOnly]
		public NativeArray<int2> offsets2D;

		[ReadOnly]
		public int numParticles;

		[ReadOnly]
		public float deltaTime;

		[ReadOnly]
		public float targetDensity;

		[ReadOnly]
		public float pressureMultiplier;

		[ReadOnly]
		public float nearPressureMultiplier;

		[ReadOnly]
		public float spikyPow2DerivativeScalingFactor;

		[ReadOnly]
		public float spikyPow3DerivativeScalingFactor;

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
			var originCell = SpatialHashingUtility.GetCell2D(pos, smoothingRadius);
			var sqrRadius = smoothingRadius * smoothingRadius;
			
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
					
					pressureForce += sharedPressure * SpatialWeightingUtility.ComputeDerivativeSpikyPow2(spikyPow2DerivativeScalingFactor, dst, smoothingRadius) * dirToNeighbour / neighbourDensity;
					pressureForce += sharedNearPressure * SpatialWeightingUtility.ComputeDerivativeSpikyPow3(spikyPow3DerivativeScalingFactor, dst, smoothingRadius) * dirToNeighbour / neighbourNearDensity;
				}
			}

			var acceleration = pressureForce / density;
			velocities[index] += acceleration * deltaTime;
		}
		
		private readonly float PressureFromDensity(float density)
		{
			return (density - targetDensity) * pressureMultiplier;
		}

		private readonly float NearPressureFromDensity(float nearDensity)
		{
			return nearPressureMultiplier * nearDensity;
		}
	}
}