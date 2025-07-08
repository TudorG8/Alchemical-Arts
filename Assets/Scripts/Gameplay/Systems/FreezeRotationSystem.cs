using PotionCraft.Gameplay.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace PotionCraft.Gameplay.Systems
{
	[UpdateInGroup(typeof(LateSimulationSystemGroup))]
	partial struct FreezeRotationSystem : ISystem
	{
		[BurstCompile]
		public void OnUpdate(ref SystemState state)
		{
			foreach (var (velocity, localTransform) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<LocalTransform>>())
			{
				var angular = velocity.ValueRW.Angular;
				angular.x = 0;
				angular.y = 0;
				velocity.ValueRW.Angular = angular;

				var linear = velocity.ValueRW.Linear;
				linear.z = 0f;
				velocity.ValueRW.Linear = linear;

				var position = localTransform.ValueRO;
				position.Position.z = 0f;
				localTransform.ValueRW = position;

				var rotation = localTransform.ValueRW.Rotation;
				var angleZ = math.atan2(
					2f * (rotation.value.w * rotation.value.z + rotation.value.x * rotation.value.y),
					1f - 2f * (rotation.value.y * rotation.value.y + rotation.value.z * rotation.value.z)
				);
				localTransform.ValueRW.Rotation = quaternion.RotateZ(angleZ);
			}
		}
	}
}