using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZoneControlState : AIState
{
    private CaptureZone currentZone;
    private Vector3 zoneWanderTarget;
    private float wanderRadius = 2.5f;
    private float wanderChangeTime = 2f;
    private float wanderTimer = 0f;
    private float shootCooldown = 0.15f;
    private float nextShootTime;
    private bool shooting = false;

    public AiStateId GetId() => AiStateId.ZoneControl;

    public void Enter(AIAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
        currentZone = FindCurrentZone(agent);
        PickNewWanderTarget(agent);
        wanderTimer = wanderChangeTime;
    }

    private void ShootAtPlayerIfVisible(AIAgent agent)
    {
        Debug.Log($"[AI] {agent.name}: ShootAtPlayerIfVisible called (ZoneControlState)");
        if (agent.player != null && agent.firePoint != null && agent.bulletPrefab != null && agent.IsTargetInFOV(agent.player))
        {
            Vector3 rayOrigin = agent.firePoint.position;
            Vector3 targetPos = agent.player.position + Vector3.up * 0.9f;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;

            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.player.position);
            float minSpread = 2f;
            float maxSpread = 7f;
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
            Debug.DrawRay(rayOrigin, rayDir * agent.aIAgentConfig.maxSightDistance, Color.red, 0.1f);
            if (canSee && Time.time >= nextShootTime)
            {
                agent.Shoot();
                nextShootTime = Time.time + shootCooldown;
            }
        }
    }

    public void Update(AIAgent agent)
    {
        Debug.Log($"[AI] {agent.name}: AIZoneControlState.Update");
        ShootAtPlayerIfVisible(agent);
        // Якщо зона вже захоплена ворогами — виходимо у Patrol
        if (currentZone == null || currentZone.currentOwner == CaptureZone.Team.Enemy)
        {
            agent.stateMachine.ChangeState(AiStateId.Patrol);
            return;
        }
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

    public void Exit(AIAgent agent) { }

    private void PickNewWanderTarget(AIAgent agent)
    {
        if (currentZone == null) return;
        Vector3 center = currentZone.transform.position;
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        zoneWanderTarget = center + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    private CaptureZone FindCurrentZone(AIAgent agent)
    {
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        foreach (var zone in zones)
        {
            if (zone.enemiesInZone.Contains(agent.transform))
                return zone;
        }
        return null;
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
} 