using System.Collections.Generic;
using UnityEngine;

namespace PotionCraft.Gameplay.Behaviours
{
	public class WrigglerBehaviour : MonoBehaviour
	{
		public GameObject Liquid;

		public new Rigidbody rigidbody;

		public new Rigidbody2D rigidbody2D;

		public List<Transform> targets;

		public int index;

		public float MovementSpeed = 100;

		public int LimitPerSpawner = 100;

		public float SpawnerDelay = 0.1f;

		public float RotationSpeed = 60;


		private void FixedUpdate()
		{
			var currentTarget = targets[index];
			var direction = (currentTarget.transform.position - transform.position).normalized;
			if (Vector3.Distance(currentTarget.transform.position, transform.position) < 0.1f)
			{
				index = (index + 1) % targets.Count;
			}
			
			if (rigidbody != null)
				rigidbody.linearVelocity = MovementSpeed * Time.fixedDeltaTime * direction;

			if (rigidbody2D != null)
				rigidbody2D.linearVelocity = MovementSpeed * Time.fixedDeltaTime * direction;

			transform.Rotate(0, 0, RotationSpeed * Time.fixedDeltaTime);
		}
	}
}