using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackPlayerState : AIState
{
	public void Enter(AIAgent agent)
	{
		if (agent.hasGun)
		{
			agent.gun.SetTarget(agent.player);
			agent.navMeshAgent.stoppingDistance = 5.0f;
			//agent.gun.SetFiring(true);
		}
	}

	public void Exit(AIAgent agent)
	{
	}

	public AiStateId GetId()
	{
		return AiStateId.AttackPlayer;
	}

	public void Update(AIAgent agent)
	{
		agent.navMeshAgent.destination = agent.player.position;
	}

}
