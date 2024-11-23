using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth;
    AIAgent agent;

	void Start()
	{
        currentHealth = maxHealth;
        agent = GetComponent<AIAgent>();
        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodies)
		{
            HitBox hitBox = rigidBody.gameObject.AddComponent<HitBox>();
            hitBox.enemyHealth = this;
		}
	}
	public void TakeDamage(float damage, Vector3 direction)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die(direction);
        }
    }

    void Die(Vector3 direction)
    {
        AIDestroyState destroyState = agent.stateMachine.GetState(AiStateId.Destroy) as AIDestroyState;
        destroyState.direction = direction;
        agent.stateMachine.ChangeState(AiStateId.Destroy);
        // Знайти CaptureZone, в якій знаходився ворог
        CaptureZone captureZone = FindCaptureZone();

        if (captureZone != null)
        {
            // Викликати метод для оновлення списків
            captureZone.OnEntityDestroyed(transform, "Enemy");
        }
        // Знищити ворога
       // Destroy(gameObject);
    }

    CaptureZone FindCaptureZone()
    {
        // Знаходимо зону захоплення за допомогою тригерів або близькості
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // Радіус перевірки
        foreach (Collider collider in colliders)
        {
            CaptureZone zone = collider.GetComponent<CaptureZone>();
            if (zone != null)
            {
                return zone;
            }
        }
        return null; // Не знайшли зону
    }
}
