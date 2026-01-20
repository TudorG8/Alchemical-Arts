using AlchemicalArts.Core.Fluid.Interaction.Models;
using UnityEngine;

namespace AlchemicalArts.Gameplay.Tools.Data
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "FluidDragSettings", menuName = "AlchemicalArts/UI/FluidDragSettings", order = 0)]
	public class FluidDragSettings : ScriptableObject
	{
		[field:SerializeField]
		public DraggingParticlesMode Mode { get; private set; }

		[field:SerializeField]
		public string DisplayText { get; private set; }

		[field:SerializeField]
		public Color Color { get; private set; }
	}	
}