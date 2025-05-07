using System.Collections.Generic;
using UnityEngine;

namespace PotionCraft.Gameplay.Behaviours
{
	public class WrigglerBehaviour : MonoBehaviour
	{
		public GameObject liquid;

		public Rigidbody rigidbody;

		public Rigidbody2D rigidbody2D;

		public List<Transform> targets;

		public int index;

		public float speed = 100;

		public int limitPerSpawner = 100;

		public float spawnerDelay = 0.1f;

		public float rotationSpeed = 60;


		private void FixedUpdate()
		{
			var currentTarget = targets[index];
			var direction = (currentTarget.transform.position - transform.position).normalized;
			if (Vector3.Distance(currentTarget.transform.position, transform.position) < 0.1f)
			{
				index = (index + 1) % targets.Count;
			}
			
			if (rigidbody != null)
				rigidbody.linearVelocity = speed * Time.fixedDeltaTime * direction;

			if (rigidbody2D != null)
				rigidbody2D.linearVelocity = speed * Time.fixedDeltaTime * direction;

			transform.Rotate(0, 0, rotationSpeed * Time.fixedDeltaTime);
		}
	}
}