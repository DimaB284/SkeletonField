using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float damage = 10f;
	public float lifetime = 3f;
	public AIAgent.Faction shooterFaction;

	void Start()
	{
		Destroy(gameObject, lifetime);
	}

	/*private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
			if (playerHealth != null)
			{
				playerHealth.TakeDamage(damage);
			}
			Destroy(gameObject);
		}
	}*/
	private void OnCollisionEnter(Collision collision)
	{
		// Наносимо урон відповідній цілі (гравець / ворог / союзник)
		Vector3 hitDir = (collision.contacts.Length > 0)
			? -collision.contacts[0].normal
			: transform.forward;

		var playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
		if (playerHealth != null)
		{
			playerHealth.TakeDamage(damage);
			Destroy(gameObject);
			return;
		}

		var enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
		if (enemyHealth != null && shooterFaction != AIAgent.Faction.Enemy)
		{
			enemyHealth.TakeDamage(damage, hitDir);
			Destroy(gameObject);
			return;
		}

		var allyHealth = collision.gameObject.GetComponentInParent<AllyHealth>();
		if (allyHealth != null && shooterFaction != AIAgent.Faction.Ally)
		{
			allyHealth.TakeDamage(damage, hitDir);
			Destroy(gameObject);
			return;
		}

		// Якщо влучили у будь-що інше — просто знищити кулю
		Destroy(gameObject);
	}
}
