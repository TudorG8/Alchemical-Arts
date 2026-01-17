using AlchemicalArts.Core.SpatialPartioning.Components;
using AlchemicalArts.Core.SpatialPartioning.Utility;
using AlchemicalArts.Gameplay.Temperature.Components;
using AlchemicalArts.Gameplay.Temperature.Models;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AlchemicalArts.Gameplay.Temperature.Jobs
{
	[BurstCompile]
	public partial struct TransferTemperatureJob : IJobEntity
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<TemperatureState> temperatureStateBuffer;

		[ReadOnly]
		public NativeArray<TemperatureSpatialEntry> spatial;
		
		[ReadOnly]
		public NativeArray<int> spatialOffsets;

		[ReadOnly]
		public NativeArray<float2> predictedPositions;

		[ReadOnly]
		public SpatialPartioningConfig spatialPartioningConfig;

		[ReadOnly]
		public SpatialPartioningConstantsConfig spatialPartioningConstantsConfig;

		[ReadOnly]
		public int numParticles;

		[ReadOnly]
		public int hashingLimit;

		public void Execute(
			in SpatiallyPartionedIndex spatiallyPartionedItemState,
			in TemperaturePartionedIndex temperaturePartionedIndex,
			in TemperatureEnabledTransmissionTag temperatureEnabledTransmissionTag)
		{
			var deltaTime = 1 / 120f;
			var position = predictedPositions[spatiallyPartionedItemState.index];
			var originCell = SpatialHashingUtility.GetCell2D(position, spatialPartioningConfig.radius);
			var sqrRadius = spatialPartioningConfig.radius * spatialPartioningConfig.radius;

			foreach (var offset in spatialPartioningConstantsConfig.offsets)
			{
				var hash = SpatialHashingUtility.HashCell2D(originCell + offset);
				var key = SpatialHashingUtility.KeyFromHash(hash, hashingLimit);
				var currIndex = spatialOffsets[key];

				while (currIndex < numParticles)
				{
					var neighbourIndex = currIndex;
					var neighbourSimulationIndex = spatial[neighbourIndex].simulationIndex;
					var neighbourTemperatureIndex = spatial[neighbourIndex].simulationIndex;
					currIndex++;

					if (neighbourIndex == spatiallyPartionedItemState.index) continue;

					var neighbourKey = spatial[neighbourIndex].key;
					if (neighbourKey != key) break;

					var neighbourPos = predictedPositions[neighbourSimulationIndex];
					var offsetToNeighbour = neighbourPos - position;
					var sqrDstToNeighbour = math.dot(offsetToNeighbour, offsetToNeighbour);

					if (sqrDstToNeighbour > sqrRadius) continue;

					var temperatureDifference = temperatureStateBuffer[temperaturePartionedIndex.Index].temperature - temperatureStateBuffer[neighbourTemperatureIndex].temperature;
					var transfer = temperatureDifference * deltaTime * math.sqrt(80 * 80);
					var inputState = temperatureStateBuffer[temperaturePartionedIndex.Index];
					var neighbourState = temperatureStateBuffer[neighbourTemperatureIndex];

					inputState.temperature -= transfer;
					neighbourState.temperature += transfer;

					temperatureStateBuffer[temperaturePartionedIndex.Index] = inputState;
					temperatureStateBuffer[neighbourTemperatureIndex] = neighbourState;
				}
			}
		}
	}
}