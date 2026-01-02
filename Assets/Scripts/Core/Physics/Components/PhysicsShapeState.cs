
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Components
{
	public struct PhysicsShapeState : IComponentData, IEnableableComponent
	{
		public PhysicsShape physicsShape;
	}
}