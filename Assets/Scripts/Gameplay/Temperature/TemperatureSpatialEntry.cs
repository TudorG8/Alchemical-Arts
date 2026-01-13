using System.Collections.Generic;
using UnityEngine;

public struct TemperatureSpatialEntry
{
	public int key;

	public int simulationIndex;

	public int temperatureIndex;
}

public struct TemperatureSpatialEntryComparer : IComparer<TemperatureSpatialEntry>
{
	public readonly int Compare(TemperatureSpatialEntry a, TemperatureSpatialEntry b)
		=> a.key.CompareTo(b.key);
}