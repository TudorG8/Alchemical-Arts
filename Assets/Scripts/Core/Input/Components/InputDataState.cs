using Unity.Entities;
using Unity.Mathematics;

namespace PotionCraft.Core.Input.Components
{
	public struct InputDataState : IComponentData
	{
		public float2 screenPosition;

		public float2 worldPosition;

		public bool primaryPressed;

		public bool secondaryPressed;

		public float scrollDelta;
	}
}