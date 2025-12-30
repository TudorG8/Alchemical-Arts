using PotionCraft.Core.Fluid.Display.Components;
using PotionCraft.Shared.Utility;
using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Display.Authoring
{
	public class FluidSimulationAuthoring : MonoBehaviour
	{
		public Shader shader;

		public Mesh mesh;

		public Gradient gradient;

		public float maxVelocity;

		public int gradientResolution;
		
		public float particleSize;
	}

	public class FluidSimulationAuthoringBaker : Baker<FluidSimulationAuthoring>
	{
		public override void Bake(FluidSimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponentObject(entity, new FluidSimulationConfig() 
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