using PotionCraft.Core.Physics.Components;
using PotionCraft.Core.Physics.Extensions;
using PotionCraft.Shared.Extensions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PotionCraft.Core.Physics.Systems
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	partial struct PhysicsSyncTransformSystem : ISystem
	{
		[BurstCompile]
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<PhysicsBodyConfigComponent>();
		}

		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			var entityManager = state.EntityManager;

			foreach (var (bodyConfig, localTransform, entity) in SystemAPI.Query<PhysicsBodyConfigComponent, RefRW<LocalTransform>>().WithEntityAccess())
			{
				var position = bodyConfig.physicsBody.position.ToFloat3();
				var rotation = bodyConfig.physicsBody.rotation.ToECSQuaternion();

				var localPosition = float3.zero;
				var localRotation = quaternion.identity;



				// if (state.EntityManager.HasComponent<Parent>(entity))
				// {
				// 	var parentEntity = entityManager.GetComponentData<Parent>(entity).Value;
				// 	if (entityManager.HasComponent<LocalToWorld>(parentEntity))
				// 	{
				// 		var parentL2W = entityManager.GetComponentData<LocalToWorld>(parentEntity);
				// 		var parentWorld = parentL2W.Value;

				// 		// Invert parent’s world transform
				// 		var parentWorldInv = math.inverse(parentWorld);

				// 		// Convert world-space to local-space
				// 		var worldTransform = float4x4.TRS(position, rotation, new float3(1, 1, 1));
				// 		var localMatrix = math.mul(parentWorldInv, worldTransform);

				// 		// Extract position & rotation
				// 		localPosition = localMatrix.c3.xyz;
				// 		localRotation = new quaternion(localMatrix);
				// 	}
				// 	else
				// 	{
				// 		// No valid LocalToWorld (shouldn’t happen but safe fallback)
				// 		localPosition = position;
				// 		localRotation = rotation;
				// 	}
				// }
				// else
				// {
				// 	// No parent, world = local
				// 	localPosition = position;
				// 	localRotation = rotation;
				// }
				
				localTransform.ValueRW.Position = position;
				localTransform.ValueRW.Rotation = rotation;
			}
		}
	}
}