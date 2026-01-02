using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Components
{
	public struct PhysicsCircleSetup : IComponentData
	{
		public PhysicsShapeDefinition shapeDefinition;

		public CircleGeometry circleGeometry;

		public Entity bodyEntity;
	}
}