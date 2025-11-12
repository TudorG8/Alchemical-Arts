
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Components
{
	public struct PhysicsShapeConfigComponent : IComponentData
	{
		public PhysicsShape physicsShape;
	}
}