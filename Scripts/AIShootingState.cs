using System.Collections;
using UnityEngine;

public class AIShootingState : AIState
{
	private float shootCooldown = 0.15f; // затримка між пострілами
	private float nextShootTime;
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
		if (target != null && agent.firePoint != null && agent.bulletPrefab != null && agent.IsTargetInFOV(target))
		{
			Vector3 directionToTarget = target.position - agent.firePoint.position;
			float distanceToTarget = directionToTarget.magnitude;

			// Поворот лише firePoint (або weaponIK) у бік цілі
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
			// Raycast для перевірки видимості
			RaycastHit hit;
			bool canSee = false;
			if (Physics.Raycast(rayOrigin, rayDir, out hit, agent.aIAgentConfig.maxSightDistance))
			{
				if (hit.transform == target)
					canSee = true;
			}
			if (canSee && Time.time >= nextShootTime)
			{
				agent.Shoot();
				nextShootTime = Time.time + shootCooldown;
			}
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