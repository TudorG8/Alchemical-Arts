using System.Collections.Generic;
using Unity.Entities;

namespace AlchemicalArts.Core.SpatialPartioning.Models
{
	public struct FluidSpatialEntry
	{
		public int key;

		public int simulationIndex;

		public int fluidIndex;
	}

	public struct SpatialEntry
	{
		public int key;

		public int simulationIndex;

		public Entity entity;
	}

	public struct SpatialEntryKeyComparer : IComparer<SpatialEntry>
	{
		public readonly int Compare(SpatialEntry a, SpatialEntry b)
			=> a.key.CompareTo(b.key);
	}
}