using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth;
    AIAgent agent;
    //CaptureZone.Team team;

	void Start()
	{
        //team = GetComponent<CaptureZone.Team>();
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
        // ������ CaptureZone, � ��� ���������� �����
        CaptureZone captureZone = FindCaptureZone();

        if (captureZone != null)
        {
            captureZone.OnEntityDestroyed(transform, "Enemy");
        }
        agent.gameObject.GetComponent<CapsuleCollider>().enabled = false;
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDeath(CaptureZone.Team.Enemy);
        }
		// Destroy(gameObject);
	}

    CaptureZone FindCaptureZone()
    {

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            CaptureZone zone = collider.GetComponent<CaptureZone>();
            if (zone != null)
            {
                return zone;
            }
        }
        return null;
    }
}
