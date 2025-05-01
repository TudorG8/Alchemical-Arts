using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class WrigglerAuthoring : MonoBehaviour
{
	public float moveSpeed;
	
	public float rotationSpeed;

	public List<Transform> waypoints;


	class WrigglerAuthoringBaker : Baker<WrigglerAuthoring>
	{
		public override void Bake(WrigglerAuthoring authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			AddComponent(entity, new Wriggler() 
			{ 
				moveSpeed = authoring.moveSpeed, 
				rotationSpeed = authoring.rotationSpeed
			});
			var buffer = AddBuffer<WrigglerTargetBuffer>(entity);
			foreach (var waypoint in authoring.waypoints)
			{
				buffer.Add(new WrigglerTargetBuffer
				{
					waypoint = waypoint.position
				});
			}
		}
	}
}

public struct Wriggler : IComponentData
{
	public float moveSpeed;

	public float rotationSpeed;

	public int index;
}

public struct WrigglerTargetBuffer : IBufferElementData
{
	public float3 waypoint;
}
