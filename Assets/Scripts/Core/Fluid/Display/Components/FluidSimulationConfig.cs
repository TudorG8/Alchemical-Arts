using Unity.Entities;
using UnityEngine;

namespace AlchemicalArts.Core.Fluid.Display.Components
{
	public class FluidSimulationConfig : IComponentData
	{
		public Shader shader;

		public Mesh mesh;

		public Texture2D texture;
		
		public float maxVelocity;
		
		public float particleSize;
	}
}