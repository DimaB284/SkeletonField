using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIdleState : AIState
{
	public void Enter(AIAgent agent)
	{
		
	}

	public void Exit(AIAgent agent)
	{

	}

	public AiStateId GetId()
	{
		return AiStateId.Idle;
	}

	public void Update(AIAgent agent)
	{
		if (CanSeePlayer(agent))
		{
			agent.stateMachine.ChangeState(AiStateId.Shooting);
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
