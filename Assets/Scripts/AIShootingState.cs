using System.Collections;
using UnityEngine;

public class AIShootingState : AIState
{
	private int burstCount = 0;
	private int burstSize = 4;
	private float burstCooldown = 1.0f;
	private float shootCooldown = 0.15f; // затримка між пострілами
	private float nextShootTime = 0f;
	private float nextBurstTime = 0f;
	private bool inBurst = false;
	private float turnSpeed = 8f; // швидкість розвороту
	private float aimSpread = 2f; // розкид у градусах

	public void Enter(AIAgent agent)
	{
		nextShootTime = Time.time + shootCooldown;
		if (agent.animator != null)
		{
			agent.animator.SetBool("isShooting", true);
		}
	}

	public void Update(AIAgent agent)
	{
		Debug.Log($"[AI] {agent.name}: AIShootingState.Update");
		var target = agent.FindTarget();
		if (target == null || !agent.IsTargetInFOV(target))
		{
			// Якщо цілі немає — повернутись у Patrol або ZoneControl
			if (agent.stateMachine != null)
			{
				// Якщо є зона — повернутись у ZoneControl, інакше Patrol
				var zone = GameObject.FindObjectOfType<CaptureZone>();
				if (zone != null && zone.playersInZone.Contains(agent.transform) || zone.enemiesInZone.Contains(agent.transform))
					agent.stateMachine.ChangeState(AiStateId.ZoneControl);
				else
					agent.stateMachine.ChangeState(AiStateId.Patrol);
			}
			if (agent.navMeshAgent != null)
				agent.navMeshAgent.isStopped = false;
			inBurst = false;
			return;
		}
		if (target != null && agent.firePoint != null && agent.bulletPrefab != null && agent.IsTargetInFOV(target))
		{
			// Зупинити рух під час стрільби
			if (agent.navMeshAgent != null)
				agent.navMeshAgent.isStopped = true;

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
					return;
				}
			}

			if (Time.time >= nextShootTime)
			{
				Vector3 directionToTarget = target.position - agent.firePoint.position;
				float distanceToTarget = directionToTarget.magnitude;
				Vector3 rayOrigin = agent.firePoint.position;
				Vector3 targetPos = target.position + Vector3.up * 0.9f;
				Vector3 rayDir = (targetPos - rayOrigin).normalized;
				float minSpread = 1f;
				float maxSpread = 10f;
				float spread = Mathf.Lerp(minSpread, maxSpread, distanceToTarget / agent.aIAgentConfig.maxSightDistance);
				Quaternion randomSpread = Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
				if (agent.weaponIK != null)
				{
					var method = agent.weaponIK.GetType().GetMethod("SetLookTarget");
					if (method != null)
					{
						method.Invoke(agent.weaponIK, new object[] { target.position });
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
					if (hit.transform == target)
						canSee = true;
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
		else
		{
			// Відновити рух, якщо цілі немає
			if (agent.navMeshAgent != null)
				agent.navMeshAgent.isStopped = false;
			inBurst = false;
		}
	}

	private bool CanSeePlayer(AIAgent agent)
	{
		var target = agent.FindTarget();
		if (target == null) return false;
		Vector3 dirToTarget = target.position - agent.transform.position;
		float dist = dirToTarget.magnitude;
		if (dist > agent.aIAgentConfig.maxSightDistance) return false;
		dirToTarget.Normalize();
		RaycastHit hit;
		if (Physics.Raycast(agent.transform.position + Vector3.up, dirToTarget, out hit, agent.aIAgentConfig.maxSightDistance))
		{
			if (hit.transform == target)
				return true;
		}
		return false;
	}

	public void Exit(AIAgent agent) { 
		if (agent.animator != null)
		{
			agent.animator.SetBool("isShooting", false);
		}
	}

	public AiStateId GetId() => AiStateId.Shooting;
}