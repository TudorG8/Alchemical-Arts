using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Components
{
	public struct PhysicsCircleSetupComponent : IComponentData
	{
		public PhysicsShapeDefinition shapeDefinition;

		public CircleGeometry circleGeometry;

		public Entity bodyEntity;
	}
}