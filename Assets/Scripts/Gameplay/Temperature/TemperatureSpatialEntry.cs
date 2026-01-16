using System.Collections.Generic;
using AlchemicalArts.Core.SpatialPartioning.Components;

public struct TemperatureSpatialEntry : ISpatialEntry, ISpatialEntryIndexer
{
	public int key;

	public int simulationIndex;

	public int temperatureIndex;


	public int Key { get => key; set => key = value; }
		
	public int SimulationIndex { get => simulationIndex; set => simulationIndex = value; }
		
	public int Index { get => temperatureIndex; set => temperatureIndex = value; }
}

public struct TemperatureSpatialEntryComparer : IComparer<TemperatureSpatialEntry>
{
	public readonly int Compare(TemperatureSpatialEntry a, TemperatureSpatialEntry b)
		=> a.key.CompareTo(b.key);
}