using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PotionCraft.Gameplay.Authoring
{
	public struct _WrigglerData : IComponentData
	{
		public float MovementSpeed;

		public float RotationSpeed;

		public int WaypointIndex;

		public int SpawnLimit;
	}

	public struct _WrigglerTargetBufferData : IBufferElementData
	{
		public float3 WaypointLocation;
	}

	public class WrigglerAuthoring : MonoBehaviour
	{
		[field: SerializeField]
		public int Limit { get; private set; }
		
		[field: SerializeField]
		public GameObject Liquid { get; private set; }
		
		[field: SerializeField]
		public float Cooldown { get; private set; }


		[field: SerializeField]
		private float MoveSpeed  { get; set; }
		
		[field: SerializeField]
		private float RotationSpeed  { get; set; }
		
		[field: SerializeField]
		private List<Transform> Waypoints { get; set; }


		public class WrigglerAuthoringBaker : Baker<WrigglerAuthoring>
		{
			public override void Bake(WrigglerAuthoring authoring)
			{
				var entity = GetEntity(TransformUsageFlags.Dynamic);
				AddComponent(entity, new _WrigglerData() 
				{ 
					MovementSpeed = authoring.MoveSpeed, 
					RotationSpeed = authoring.RotationSpeed,
					SpawnLimit = authoring.Limit,
				});
				var buffer = AddBuffer<_WrigglerTargetBufferData>(entity);
				foreach (var waypoint in authoring.Waypoints)
				{
					buffer.Add(new _WrigglerTargetBufferData
					{
						WaypointLocation = waypoint.position
					});
				}
			}
		}
	}
}