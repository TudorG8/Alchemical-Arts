using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Components
{
	public struct PhysicsBoxSetup : IComponentData
	{
		public PhysicsShapeDefinition shapeDefinition;

		public Entity bodyEntity;
	}
}