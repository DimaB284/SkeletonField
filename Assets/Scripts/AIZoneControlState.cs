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
    private int burstCount = 0;
    private int burstSize = 4;
    private float burstCooldown = 1.0f;
    private float shootCooldown = 0.15f;
    private float nextShootTime = 0f;
    private float nextBurstTime = 0f;
    private bool inBurst = false;
    private bool shooting = false;
    // Обмеження повороту/прицілювання та оновлення цілі, щоб уникнути "п’яного" бігу
    private float rotateSpeedDeg = 180f;   // швидкість повороту тіла до цілі (deg/sec)
    private float aimConeEnterAngle = 75f; // вхідний поріг
    private float aimConeExitAngle = 95f;  // вихідний поріг (гістерезис)
    private bool withinAimConeSticky = false;
    private Vector3 lastSetDestination = Vector3.zero;
    private float nextDestinationUpdateTime = 0f;
    private float destinationUpdateInterval = 0.3f;
    private float destinationRecalcDistanceSqr = 0.25f; // ~0.5м

    public AiStateId GetId() => AiStateId.ZoneControl;

    public void Enter(AIAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
        currentZone = FindCurrentZone(agent);
        PickNewWanderTarget(agent);
        wanderTimer = wanderChangeTime;
        withinAimConeSticky = false;
        nextDestinationUpdateTime = 0f;
    }

    private void EngageTargetIfVisible(AIAgent agent)
    {
        // Обираємо найближчу ціль супротивника (для Ally — Enemy, для Enemy — Ally/Player)
        Transform target = agent.FindTarget();
        if (target == null)
        {
            inBurst = false;
            // Якщо немає цілі — легке "гуляння" всередині зони
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f || Vector3.Distance(agent.transform.position, zoneWanderTarget) < 1f)
            {
                PickNewWanderTarget(agent);
                wanderTimer = wanderChangeTime;
            }
            if (agent.navMeshAgent != null && agent.navMeshAgent.enabled && agent.navMeshAgent.isOnNavMesh)
            {
                agent.navMeshAgent.SetDestination(zoneWanderTarget);
            }
            return;
        }

        // Якщо є ціль — підходимо до неї та стріляємо, якщо бачимо
        // Не змінюємо навігаційний маршрут на ворога, щоб уникнути "смикання" — агент продовжує свій поточний маршрут у зоні

        if (agent.firePoint == null || agent.bulletPrefab == null)
            return;

        // Перевірка сектора огляду
        bool inFov = agent.IsTargetInFOV(target);
        if (!inFov)
        {
            inBurst = false; // поки не в полі зору — не стріляємо серіями, але продовжуємо зближення
        }

        // Обчислення горизонтального кута до цілі
        Vector3 toTarget = target.position - agent.transform.position;
        toTarget.y = 0f;
        Vector3 fwd = agent.transform.forward;
        fwd.y = 0f;
        float horizontalAngle = Vector3.Angle(fwd, toTarget);

        // Гістерезис для стабільності, щоб не "смикалось"
        if (!withinAimConeSticky && horizontalAngle <= aimConeEnterAngle)
            withinAimConeSticky = true;
        else if (withinAimConeSticky && horizontalAngle > aimConeExitAngle)
            withinAimConeSticky = false;

        // Плавно повертаємо корпус коли агент майже стоїть (щоб при статичній перестрілці вони дивились один на одного)
        if (agent.navMeshAgent != null && agent.navMeshAgent.velocity.sqrMagnitude < 0.05f && toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, targetRot, rotateSpeedDeg * Time.deltaTime);
        }

        if (!inBurst)
        {
            if (Time.time >= nextBurstTime)
            {
                inBurst = true;
                burstCount = 0;
                burstSize = Random.Range(3, 6);
                shootCooldown = Random.Range(0.09f, 0.15f);
                burstCooldown = Random.Range(0.8f, 1.3f);
            }
            else
            {
                return; // Чекаємо паузу між серіями
            }
        }

        if (Time.time >= nextShootTime && withinAimConeSticky && inFov)
        {
            Vector3 rayOrigin = agent.firePoint.position;
            Vector3 targetPos = target.position + Vector3.up * 0.9f;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;
            float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
            float minSpread = 2f;
            float maxSpread = 7f;
            float spread = Mathf.Lerp(minSpread, maxSpread, distanceToTarget / agent.aIAgentConfig.maxSightDistance);
            // Дуже малий розкид на близькій дистанції, і нульовий на <2м
            if (distanceToTarget < 2f)
            {
                minSpread = 0f; maxSpread = 0f; spread = 0f;
            }
            else if (distanceToTarget < 6f)
            {
                minSpread = 0.3f;
                maxSpread = 1.2f;
                spread = Mathf.Lerp(minSpread, maxSpread, (distanceToTarget - 2f) / 4f);
            }
            Quaternion randomSpread = Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
            if (agent.weaponIK != null)
            {
                // Перевага методу SetTargetTransform(Transform), якщо він є; інакше SetLookTarget(Vector3)
                var ikType = agent.weaponIK.GetType();
                var setTargetTransform = ikType.GetMethod("SetTargetTransform");
                if (setTargetTransform != null)
                    setTargetTransform.Invoke(agent.weaponIK, new object[] { target });
                else
                {
                    var setLookTarget = ikType.GetMethod("SetLookTarget");
                    if (setLookTarget != null)
                        setLookTarget.Invoke(agent.weaponIK, new object[] { target.position });
                }
            }
            // Гарантовано вирівнюємо сам firePoint у напрямку пострілу, але не дозволяємо roll/pitch відхиленням (тільки yaw)
            Vector3 flatDir = rayDir; flatDir.y = 0f; if (flatDir.sqrMagnitude > 0.0001f) flatDir.Normalize();
            Quaternion yawOnly = Quaternion.LookRotation(flatDir, Vector3.up);
            agent.firePoint.rotation = yawOnly * randomSpread;
            RaycastHit hit;
            bool canSee = false;
            if (Physics.Raycast(rayOrigin, rayDir, out hit, agent.aIAgentConfig.maxSightDistance))
            {
                if (hit.transform == target
                    || hit.transform.root == target
                    || hit.transform.CompareTag(target.tag))
                {
                    canSee = true;
                }
                else
                {
                    var hitAgent = hit.transform.GetComponentInParent<AIAgent>();
                    if (hitAgent != null && hitAgent.transform == target)
                        canSee = true;
                }
            }
            Debug.DrawRay(rayOrigin, rayDir * agent.aIAgentConfig.maxSightDistance, Color.red, 0.1f);
            if (canSee)
            {
                agent.Shoot();
                burstCount++;
                nextShootTime = Time.time + shootCooldown;
                if (burstCount >= burstSize)
                {
                    inBurst = false;
                    nextBurstTime = Time.time + burstCooldown + Random.Range(0f, 0.3f);
                }
            }
        }
    }

    public void Update(AIAgent agent)
    {
        Debug.Log($"[AI] {agent.name}: AIZoneControlState.Update");
        EngageTargetIfVisible(agent);
        // Якщо зона вже захоплена командою агента — виходимо у Patrol, щоб пріоритезувати інші зони
        if (currentZone == null
            || (agent.faction == AIAgent.Faction.Ally && currentZone.currentOwner == CaptureZone.Team.Player)
            || (agent.faction == AIAgent.Faction.Enemy && currentZone.currentOwner == CaptureZone.Team.Enemy))
        {
            agent.stateMachine.ChangeState(AiStateId.Patrol);
            return;
        }
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        bool hasAvailableZone = false;
        foreach (var zone in zones)
        {
            if (agent.faction == AIAgent.Faction.Ally)
            {
                if (zone.currentOwner != CaptureZone.Team.Player)
                {
                    hasAvailableZone = true;
                    break;
                }
            }
            else if (agent.faction == AIAgent.Faction.Enemy)
            {
                if (zone.currentOwner != CaptureZone.Team.Enemy)
                {
                    hasAvailableZone = true;
                    break;
                }
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
            if (agent.faction == AIAgent.Faction.Ally)
            {
                if (zone.playersInZone.Contains(agent.transform))
                    return zone;
            }
            else
            {
                if (zone.enemiesInZone.Contains(agent.transform))
                    return zone;
            }
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