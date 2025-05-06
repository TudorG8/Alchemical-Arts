using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct LiquidWrigglerSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		foreach(var (localTransform, physicsVelocity, wriggler, wrigglerTargetBuffer) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRW<Wriggler>, DynamicBuffer<WrigglerTargetBuffer>>())
		{
			var angularVelocity = new float3(0, 0, wriggler.ValueRO.rotationSpeed);
			physicsVelocity.ValueRW.Angular = angularVelocity;

			var currentTarget = wrigglerTargetBuffer[wriggler.ValueRO.index];
			var direction = currentTarget.waypoint - localTransform.ValueRW.Position;
			if (math.length(direction) < 0.1f)
			{
				wriggler.ValueRW.index = (wriggler.ValueRO.index + 1) % wrigglerTargetBuffer.Length;
			}
			physicsVelocity.ValueRW.Linear = SystemAPI.Time.DeltaTime * wriggler.ValueRO.moveSpeed * direction;
		}

		// Physics.SyncTransforms();
	}
}
