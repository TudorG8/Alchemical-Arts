using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Extensions
{
	public static class PhysicsShapeDefinitionExtensions
	{
		public static PhysicsShapeDefinition WithColor(this PhysicsShapeDefinition input, Color32 color)
		{
			input.surfaceMaterial.customColor = color;
			return input;
		}
	}
}