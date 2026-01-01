using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Simulation.Models
{
	public struct SpatialEntry
	{
		public int index;
		
		public int key;
	}

	public struct SpatialEntryKeyComparer : IComparer<SpatialEntry>
	{
		public readonly int Compare(SpatialEntry a, SpatialEntry b)
			=> a.key.CompareTo(b.key);
	}
}