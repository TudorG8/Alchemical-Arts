using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Groups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(PhysicsInitializationGroup))]
	[UpdateAfter(typeof(CreatePhysicsBodySystem))]
	partial struct CreatePhysicsCircleSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsCircleSetup>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			using var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

			foreach (var (circleSetup, entity) in SystemAPI.Query<PhysicsCircleSetup>().WithEntityAccess())
			{
				var parent = SystemAPI.GetComponent<PhysicsBodyState>(circleSetup.bodyEntity);

				var physicsShape = parent.physicsBody.CreateShape(circleSetup.circleGeometry, circleSetup.shapeDefinition);
				commandBuffer.AddComponent(entity, new PhysicsShapeState() { physicsShape = physicsShape });
				commandBuffer.RemoveComponent<PhysicsCircleSetup>(entity);
			}

			commandBuffer.Playback(state.EntityManager);
		}
	}
}