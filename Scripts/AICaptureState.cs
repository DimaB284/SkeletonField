using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICaptureState : AIState
{
    private CaptureZone currentZone;
    private bool isInZone = false;
    private Vector3 zoneWanderTarget;
    private float wanderRadius = 2.5f; // радіус "гуляння" всередині зони
    private float wanderChangeTime = 2f;
    private float wanderTimer = 0f;

    public AiStateId GetId() => AiStateId.Capture;

    public void Enter(AIAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
        currentZone = FindClosestZone(agent);
        PickNewWanderTarget(agent);
        wanderTimer = wanderChangeTime;
    }

    public void Update(AIAgent agent)
    {
        Debug.Log($"[AI] {agent.name}: AICaptureState.Update");
        // Якщо бачить гравця — стріляє, але не змінює стан
        if (agent.player != null && agent.firePoint != null && agent.bulletPrefab != null && agent.IsTargetInFOV(agent.player))
        {
            Debug.Log($"[AI] {agent.name}: Shoot block entered (CaptureState)");
            Vector3 rayOrigin = agent.firePoint.position;
            Vector3 targetPos = agent.player.position + Vector3.up * 0.9f;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;

            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.player.position);
            float minSpread = 1f;
            float maxSpread = 10f;
            float spread = Mathf.Lerp(minSpread, maxSpread, distanceToPlayer / agent.aIAgentConfig.maxSightDistance);
            Quaternion randomSpread = Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);

            if (agent.weaponIK != null)
            {
                var method = agent.weaponIK.GetType().GetMethod("SetLookTarget");
                if (method != null)
                {
                    method.Invoke(agent.weaponIK, new object[] { agent.player.position });
                }
            }
            else
            {
                agent.firePoint.rotation = Quaternion.LookRotation(rayDir) * randomSpread;
            }
            RaycastHit hit;
            bool canSee = false;
            if (Physics.Raycast(rayOrigin, rayDir, out hit, agent.aIAgentConfig.maxSightDistance))
            {
                if (hit.transform == agent.player)
                    canSee = true;
            }
            if (canSee)
            {
                if (!shooting || Time.time >= nextShootTime)
                {
                    agent.Shoot();
                    nextShootTime = Time.time + shootCooldown;
                    shooting = true;
                }
            }
            else
            {
                shooting = false;
            }
        }
        else
        {
            shooting = false;
        }

        if (currentZone == null)
        {
            agent.stateMachine.ChangeState(AiStateId.Patrol);
            return;
        }

        // Якщо зона вже захоплена ворогами — патрулюємо далі
        if (currentZone.currentOwner == CaptureZone.Team.Enemy)
        {
            agent.stateMachine.ChangeState(AiStateId.Patrol);
            return;
        }

        // Гуляємо всередині зони, не виходячи з неї
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f || Vector3.Distance(agent.transform.position, zoneWanderTarget) < 1f)
        {
            PickNewWanderTarget(agent);
            wanderTimer = wanderChangeTime;
        }
        agent.navMeshAgent.SetDestination(zoneWanderTarget);

        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        bool hasAvailableZone = false;
        foreach (var zone in zones)
        {
            if (zone.currentOwner != CaptureZone.Team.Enemy)
            {
                hasAvailableZone = true;
                break;
            }
        }
        if (!hasAvailableZone)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }
    }

    public void Exit(AIAgent agent)
    {
        // Видалено видалення з enemiesInZone — це робить CaptureZone через OnTriggerExit
    }

    private CaptureZone FindClosestZone(AIAgent agent)
    {
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        float minDist = float.MaxValue;
        CaptureZone closest = null;
        foreach (var zone in zones)
        {
            float dist = Vector3.Distance(agent.transform.position, zone.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = zone;
            }
        }
        return closest;
    }

    private bool CanSeePlayer(AIAgent agent)
    {
        if (agent.player == null) return false;
        Vector3 dirToPlayer = agent.player.position - agent.transform.position;
        float dist = dirToPlayer.magnitude;
        if (dist > agent.aIAgentConfig.maxSightDistance) return false;
        dirToPlayer.Normalize();
        // Прибираю перевірку dot для 360° огляду
        RaycastHit hit;
        if (Physics.Raycast(agent.transform.position + Vector3.up, dirToPlayer, out hit, agent.aIAgentConfig.maxSightDistance))
        {
            if (hit.transform == agent.player)
                return true;
        }
        return false;
    }

    private void PickNewWanderTarget(AIAgent agent)
    {
        if (currentZone == null) return;
        Vector3 center = currentZone.transform.position;
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        zoneWanderTarget = center + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private float shootCooldown = 0.15f;
    private float nextShootTime;
    private bool shooting = false;
} 