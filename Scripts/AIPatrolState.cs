using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPatrolState : AIState
{
    private CaptureZone targetZone;
    private float changeTargetTimer;
    private float patrolRadius = 10f; // радіус для випадкових точок, якщо всі зони захоплені
    private Vector3 randomPoint;
    private bool useRandomPoint = false;
    private float shootCooldown = 0.15f;
    private float nextShootTime;
    private bool shooting = false;

    public AiStateId GetId() => AiStateId.Patrol;

    public void Enter(AIAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
        Debug.Log($"[AI] Enter Patrol for {agent.name}");
        PickNewTarget(agent);
        changeTargetTimer = Random.Range(5f, 15f);
    }

    private void ShootAtPlayerIfVisible(AIAgent agent)
    {
        if (CanSeePlayer(agent) && agent.firePoint != null && agent.bulletPrefab != null)
        {
            Vector3 rayOrigin = agent.firePoint.position;
            Vector3 targetPos = agent.player.position + Vector3.up * 1.2f;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;

            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.player.position);
            float minSpread = 2f; // мінімальний розкид (на близькій відстані)
            float maxSpread = 7f; // максимальний розкид (на далекій)
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

            // Raycast для перевірки видимості (опціонально)
            RaycastHit hit;
            bool canSee = false;
            if (Physics.Raycast(rayOrigin, rayDir, out hit, agent.aIAgentConfig.maxSightDistance))
            {
                if (hit.transform == agent.player)
                    canSee = true;
            }
            if (canSee && Time.time >= nextShootTime)
            {
                agent.Shoot();
                nextShootTime = Time.time + shootCooldown;
            }
        }
    }

    public void Update(AIAgent agent)
    {
        ShootAtPlayerIfVisible(agent);

        // Якщо цільова зона вже захоплена ворогами — вибираємо нову
        if (!useRandomPoint && targetZone != null && targetZone.currentOwner == CaptureZone.Team.Enemy)
        {
            PickNewTarget(agent);
        }

        changeTargetTimer -= Time.deltaTime;
        if (changeTargetTimer <= 0f)
        {
            PickNewTarget(agent);
            changeTargetTimer = Random.Range(5f, 15f);
        }

        // Завжди рухаємося у випадкову точку всередині зони, не у центр
        if (useRandomPoint)
        {
            agent.navMeshAgent.SetDestination(randomPoint);
            Debug.Log($"[AI] SetDestination (randomPoint): {randomPoint} for {agent.name}");
            if (Vector3.Distance(agent.transform.position, randomPoint) < 2f)
            {
                PickNewTarget(agent);
            }
        }
        else if (targetZone != null)
        {
            // Вибираємо випадкову точку всередині тригер-колайдера зони
            Vector3 destination = targetZone.transform.position;
            float zoneRadius = 0f;
            var sphere = targetZone.GetComponent<SphereCollider>();
            if (sphere != null && sphere.isTrigger)
            {
                zoneRadius = sphere.radius * targetZone.transform.lossyScale.x * 0.7f; // 0.7 щоб не на самому краю
                Vector2 randomCircle = Random.insideUnitCircle * zoneRadius;
                destination += new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            else
            {
                // Якщо не SphereCollider, пробуємо BoxCollider
                var box = targetZone.GetComponent<BoxCollider>();
                if (box != null && box.isTrigger)
                {
                    Vector3 boxSize = Vector3.Scale(box.size, targetZone.transform.lossyScale) * 0.7f; // 0.7 — запас від краю
                    Vector3 boxCenter = targetZone.transform.position + box.center;
                    float halfX = boxSize.x / 2f;
                    float halfZ = boxSize.z / 2f;
                    float randX = Random.Range(-halfX, halfX);
                    float randZ = Random.Range(-halfZ, halfZ);
                    destination = new Vector3(boxCenter.x + randX, targetZone.transform.position.y, boxCenter.z + randZ);
                }
                else
                {
                    // Fallback — рух у центр
                    Debug.LogWarning($"[AIPatrolState] {targetZone.pointName} не має SphereCollider/BoxCollider, рух у центр");
                }
            }
            agent.navMeshAgent.stoppingDistance = 0f;
            agent.navMeshAgent.SetDestination(destination);
            Debug.Log($"[AI] SetDestination (zone): {destination} for {agent.name}");
            // Не переходимо у Capture, чекаємо OnTriggerEnter
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

    public void Exit(AIAgent agent)
    {
        // Можна додати логіку при виході зі стану
    }

    private void PickNewTarget(AIAgent agent)
    {
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        List<CaptureZone> priorityZones = new List<CaptureZone>();
        foreach (var zone in zones)
        {
            if (zone.currentOwner == CaptureZone.Team.Neutral || zone.currentOwner == CaptureZone.Team.Player)
            {
                priorityZones.Add(zone);
            }
        }
        if (priorityZones.Count > 0)
        {
            targetZone = priorityZones[Random.Range(0, priorityZones.Count)];
            useRandomPoint = false;
        }
        else if (zones.Length > 0)
        {
            targetZone = zones[Random.Range(0, zones.Length)];
            useRandomPoint = false;
        }
        else
        {
            // Якщо немає зон, рухаємося у випадкову точку
            randomPoint = agent.transform.position + Random.insideUnitSphere * patrolRadius;
            randomPoint.y = agent.transform.position.y;
            useRandomPoint = true;
        }
        if (targetZone != null)
        {
            Debug.Log($"[AIPatrolState] {agent.name} вибрав зону: {targetZone.pointName}, owner: {targetZone.currentOwner}");
        }
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