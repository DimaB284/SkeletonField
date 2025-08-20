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
    private int burstCount = 0;
    private int burstSize = 4;
    private float burstCooldown = 1.0f;
    private float nextBurstTime = 0f;
    private bool inBurst = false;

    public AiStateId GetId() => AiStateId.Patrol;

    public void Enter(AIAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
        PickNewTarget(agent);
        changeTargetTimer = Random.Range(5f, 15f);
    }

    private void ShootAtOpponentIfVisible(AIAgent agent)
    {
        Transform target = agent.FindTarget();
        if (target == null || agent.firePoint == null || agent.bulletPrefab == null)
        {
            inBurst = false;
            return;
        }

        if (!agent.IsTargetInFOV(target))
        {
            inBurst = false;
            return;
        }

        // Простий конус наведення, щоб не було перебільшених поворотів при русі
        Vector3 toTarget = target.position - agent.transform.position;
        toTarget.y = 0f;
        Vector3 fwd = agent.transform.forward; fwd.y = 0f;
        float angle = Vector3.Angle(fwd, toTarget);
        if (angle > 80f) { inBurst = false; return; }

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
        if (Time.time >= nextShootTime)
        {
            Vector3 rayOrigin = agent.firePoint.position;
            Vector3 targetPos = target.position + Vector3.up * 0.9f;
            Vector3 rayDir = (targetPos - rayOrigin).normalized;
            float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
            float minSpread = 2f;
            float maxSpread = 7f;
            float spread = Mathf.Lerp(minSpread, maxSpread, distanceToTarget / agent.aIAgentConfig.maxSightDistance);
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
            // Гарантовано вирівнюємо сам firePoint у напрямку пострілу, по горизонту (yaw)
            Vector3 flatDir = rayDir; flatDir.y = 0f; if (flatDir.sqrMagnitude > 0.0001f) flatDir.Normalize();
            Quaternion yawOnly = Quaternion.LookRotation(flatDir, Vector3.up);
            agent.firePoint.rotation = yawOnly * randomSpread;
            RaycastHit hit;
            bool canSee = false;
            if (Physics.Raycast(rayOrigin, rayDir, out hit, agent.aIAgentConfig.maxSightDistance))
            {
                if (hit.transform == target || hit.transform.root == target)
                    canSee = true;
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
        Debug.Log($"[AI] {agent.name}: AIPatrolState.Update");
        // Якщо союзник — скидаємо IK, якщо ворог не в полі зору, знищений або неактивний
        if (agent.faction == AIAgent.Faction.Ally && agent.weaponIK != null)
        {
            var target = agent.FindTarget();
            bool validTarget = target != null && target.gameObject.activeInHierarchy;
            bool shouldLook = validTarget && agent.IsTargetInFOV(target);
            var method = agent.weaponIK.GetType().GetMethod("SetTargetTransform");
            if (method != null)
                method.Invoke(agent.weaponIK, new object[] { shouldLook ? target : null });
        }
        // Стріляємо по супротивнику, не змінюючи маршруту патрулювання
        ShootAtOpponentIfVisible(agent);

        // Якщо цільова зона вже захоплена командою агента — вибираємо нову ціль
        if (!useRandomPoint && targetZone != null &&
            ((agent.faction == AIAgent.Faction.Ally && targetZone.currentOwner == CaptureZone.Team.Player) ||
             (agent.faction == AIAgent.Faction.Enemy && targetZone.currentOwner == CaptureZone.Team.Enemy)))
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
            if (agent.navMeshAgent != null && agent.navMeshAgent.isOnNavMesh)
            {
                agent.navMeshAgent.SetDestination(randomPoint);
            }
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
            if (agent.navMeshAgent != null && agent.navMeshAgent.isOnNavMesh)
            {
                agent.navMeshAgent.stoppingDistance = 0f;
                agent.navMeshAgent.SetDestination(destination);
            }
            // Не переходимо у Capture, чекаємо OnTriggerEnter
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

    public void Exit(AIAgent agent)
    {
        // Можна додати логіку при виході зі стану
    }

    private void PickNewTarget(AIAgent agent)
    {
        var zones = GameObject.FindObjectsOfType<CaptureZone>();
        List<CaptureZone> priorityZones = new List<CaptureZone>();
        /*foreach (var zone in zones)
        {
            if (zone.currentOwner == CaptureZone.Team.Neutral || zone.currentOwner == CaptureZone.Team.Player)
            {
                priorityZones.Add(zone);
            }
        }*/
        foreach (var zone in zones)
{
    if (agent.faction == AIAgent.Faction.Ally)
    {
        if (zone.currentOwner == CaptureZone.Team.Neutral || zone.currentOwner == CaptureZone.Team.Enemy)
            priorityZones.Add(zone);
    }
    else if (agent.faction == AIAgent.Faction.Enemy)
    {
        if (zone.currentOwner == CaptureZone.Team.Neutral || zone.currentOwner == CaptureZone.Team.Player)
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
            // Debug.Log($"[AIPatrolState] {agent.name} вибрав зону: {targetZone.pointName}, owner: {targetZone.currentOwner}");
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