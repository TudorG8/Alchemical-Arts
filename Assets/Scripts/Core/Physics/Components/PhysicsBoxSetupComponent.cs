using Unity.Collections;
using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Components
{
	public struct PhysicsBoxSetupComponent : IComponentData
	{
		public PhysicsShapeDefinition shapeDefinition;

		public Entity bodyEntity;
	}
}