using System.Collections.Generic;
using UnityEngine;

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


	void FixedUpdate()
	{
		var currentTarget = targets[index];
		var direction = (currentTarget.transform.position - transform.position).normalized;
		if (Vector3.Distance(currentTarget.transform.position, transform.position) < 0.1f)
		{
			index = (index + 1) % targets.Count;
		}
		
		if (rigidbody != null)
			rigidbody.linearVelocity = direction * speed * Time.fixedDeltaTime;

		if (rigidbody2D != null)
			rigidbody2D.linearVelocity = direction * speed * Time.fixedDeltaTime;

		transform.Rotate(0, 0, rotationSpeed * Time.fixedDeltaTime);
	}
}
