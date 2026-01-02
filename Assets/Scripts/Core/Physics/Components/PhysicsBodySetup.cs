using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Components
{
	public struct PhysicsBodySetup : IComponentData
	{
		public PhysicsBodyDefinition bodyDefinition;
	}
}