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
	}
}
