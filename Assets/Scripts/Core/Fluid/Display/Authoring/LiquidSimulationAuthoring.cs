using PotionCraft.Core.Fluid.Display.Components;
using PotionCraft.Shared.Utility;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Display.Authoring
{
	public class LiquidSimulationAuthoring : MonoBehaviour
	{
		public Shader shader;

		public Mesh mesh;

		public Gradient gradient;

		public float maxVelocity;

		public int gradientResolution;
		
		public float particleSize;
	}

	public class LiquidSimulationAuthoringBaker : Baker<LiquidSimulationAuthoring>
	{
		public override void Bake(LiquidSimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponentObject(entity, new LiquidSimulationConfig() 
			{
				shader = authoring.shader,
				mesh = authoring.mesh,
				maxVelocity = authoring.maxVelocity,
				particleSize = authoring.particleSize,
				texture = TextureUtility.TextureFromGradient(authoring.gradientResolution, authoring.gradient)
			});
		}
	}
}