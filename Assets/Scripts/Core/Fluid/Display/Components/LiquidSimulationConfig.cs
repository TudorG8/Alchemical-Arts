using Unity.Entities;
using UnityEngine;

namespace PotionCraft.Core.Fluid.Display.Components
{
	public class LiquidSimulationConfig : IComponentData
	{
		public Shader shader;

		public Mesh mesh;

		public float maxVelocity;
		
		public float particleSize;

		public Texture2D texture;
	}
}