using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    [HideInInspector] public float currentHealth;
    private AIAgent agent;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<AIAgent>();

        // Створюємо hitbox-и на дочірніх ріджиді, щоб приймати урон від raycast-зброї за потреби
        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodies)
        {
            AllyHitBox hitBox = rigidBody.gameObject.AddComponent<AllyHitBox>();
            hitBox.allyHealth = this;
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
        if (agent != null)
        {
            AIDestroyState destroyState = agent.stateMachine.GetState(AiStateId.Destroy) as AIDestroyState;
            if (destroyState != null)
            {
                destroyState.direction = direction;
                agent.stateMachine.ChangeState(AiStateId.Destroy);
            }
        }

        // Сповістити зону, що "гравець/союзник" зник (видаляємо з playersInZone)
        CaptureZone captureZone = FindCaptureZone();
        if (captureZone != null)
        {
            captureZone.OnEntityDestroyed(transform, "Player");
        }

        // Вимикаємо колайдер, щоб не взаємодіяв після смерті
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule != null) capsule.enabled = false;

        // Додаємо у чергу респавну для команди Player (ally)
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnUnitDeath(CaptureZone.Team.Player);
        }
    }

    private CaptureZone FindCaptureZone()
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


