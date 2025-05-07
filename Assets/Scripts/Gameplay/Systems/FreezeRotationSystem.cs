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
			foreach(var (physicsMass, liquid) in SystemAPI.Query<RefRW<PhysicsMass>, _LiquidTag>())
			{
				physicsMass.ValueRW.InverseInertia = new float3(0, 0, 0);
			}

			foreach (var (velocity, localTransform, _) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<LocalTransform>, _LiquidTag>())
			{
				var angular = velocity.ValueRW.Angular;
				angular.z = 0; 
				velocity.ValueRW.Angular = angular;
				velocity.ValueRW.Linear.z = 0f;
				var tf = localTransform.ValueRO;
				tf.Position.z = 0f;
				localTransform.ValueRW = tf;
			}
		}
	}
}