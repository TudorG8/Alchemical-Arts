using Unity.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace PotionCraft.Core.Physics.Components
{
	public struct PhysicsWorldSetupComponent : IComponentData
	{
		public PhysicsWorldDefinition worldDefinition;
	}
}