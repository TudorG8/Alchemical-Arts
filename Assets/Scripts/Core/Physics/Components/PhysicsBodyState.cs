using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Components
{
	public struct PhysicsBodyState : IComponentData,  IEnableableComponent
	{
		public PhysicsBody physicsBody;
	}
}