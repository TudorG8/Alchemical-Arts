using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.LowLevelPhysics2D;

namespace AlchemicalArts.Core.Physics.Extensions
{
	public static class PhysicsBodyExtensions
	{
		public static PhysicsTransform ToPhysicsTransform(this PhysicsBody input)
		{
			return new PhysicsTransform(input.position, input.rotation);
		}

		public static PhysicsTransform ToReversePhysicsTransform(this PhysicsBody input)
		{
			var angle = (360 - input.rotation.angle * math.TODEGREES) * math.TORADIANS;
			return new PhysicsTransform(-input.position, new PhysicsRotate(angle));
		}

		public static void SetEntity(this PhysicsBody physicsBody, Entity entity) 
			=> physicsBody.userData = new PhysicsUserData { physicsMaskValue = entity.EncodeAsPhysicsMask() };
		
		public static Entity GetEntity(this PhysicsBody physicsBody) 
			=> physicsBody.userData.physicsMaskValue.DecodeFromPhysicsMask();
	}
}