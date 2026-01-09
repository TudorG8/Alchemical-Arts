using AlchemicalArts.Core.Fluid.Display.Components;
using AlchemicalArts.Shared.Utility;
using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Display.Authoring
{
	public class FluidSimulationAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public Shader Shader { get; private set; }
		
		[field: SerializeField]
		public Mesh Mesh { get; private set;}
		
		[field: SerializeField]
		public Gradient Gradient { get; private set;}
		
		[field: SerializeField]
		public float MaxVelocity { get; private set;}
		
		[field: SerializeField]
		public int GradientResolution { get; private set;}
		
		[field: SerializeField]
		public float ParticleSize { get; private set;}
	}

	public class FluidSimulationAuthoringBaker : Baker<FluidSimulationAuthoring>
	{
		public override void Bake(FluidSimulationAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.None);
			AddComponentObject(entity, new FluidSimulationDisplayConfig() 
			{
				shader = authoring.Shader,
				mesh = authoring.Mesh,
				maxVelocity = authoring.MaxVelocity,
				particleSize = authoring.ParticleSize,
				texture = TextureUtility.TextureFromGradient(authoring.GradientResolution, authoring.Gradient)
			});
		}
	}
}