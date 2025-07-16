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
		if (agent.player != null && agent.firePoint != null && agent.bulletPrefab != null)
		{
			Vector3 directionToPlayer = agent.player.position - agent.firePoint.position;
			float distanceToPlayer = directionToPlayer.magnitude;

			// Поворот лише firePoint (або weaponIK) у бік гравця
			Vector3 rayOrigin = agent.firePoint.position;
			Vector3 targetPos = agent.player.position + Vector3.up * 1.2f;
			Vector3 rayDir = (targetPos - rayOrigin).normalized;

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
			// Raycast для перевірки видимості
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

	private bool CanSeePlayer(AIAgent agent)
	{
		if (agent.player == null) return false;
		Vector3 dirToPlayer = agent.player.position - agent.transform.position;
		float dist = dirToPlayer.magnitude;
		if (dist > agent.aIAgentConfig.maxSightDistance) return false;
		dirToPlayer.Normalize();
		RaycastHit hit;
		if (Physics.Raycast(agent.transform.position + Vector3.up, dirToPlayer, out hit, agent.aIAgentConfig.maxSightDistance))
		{
			if (hit.transform == agent.player)
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