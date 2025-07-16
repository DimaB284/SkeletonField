using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIChasePlayerState : AIState
{
	//public Transform player;
	float timer = 0.0f;
	public void Enter(AIAgent agent)
	{
	}

	public void Exit(AIAgent agent)
	{
	}

	public AiStateId GetId()
	{
		return AiStateId.ChasePlayer;
	}

	public void Update(AIAgent agent)
	{
		Vector3 directionToPlayer = agent.player.position - agent.transform.position;
		float distanceToPlayer = directionToPlayer.magnitude;
		if (!agent.enabled)
		{
			return;
		}
		timer -= Time.deltaTime;
		if (!agent.navMeshAgent.hasPath)
		{
			agent.navMeshAgent.destination = agent.player.position;
		}
		if (timer < 0.0f)
		{
			Vector3 direction = (agent.player.position - agent.navMeshAgent.destination);
			direction.y = 0;
			// float sqrDistance = (player.position - agent.destination).sqrMagnitude;
			if (direction.sqrMagnitude > agent.aIAgentConfig.maxDistance * agent.aIAgentConfig.maxDistance)
			{
				if (agent.navMeshAgent.pathStatus != NavMeshPathStatus.PathPartial)
				{
					agent.navMeshAgent.destination = agent.player.position;
				}
			}
			timer = agent.aIAgentConfig.maxTime;
		}
		if (CanSeePlayer(agent))
		{
			agent.stateMachine.ChangeState(AiStateId.Shooting);
		}
	}

	// Додаю функцію CanSeePlayer
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
