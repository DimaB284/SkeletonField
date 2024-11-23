using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDestroyState : AIState
{
	public Vector3 direction;
	public void Enter(AIAgent agent)
	{

	}

	public void Exit(AIAgent agent)
	{
		
	}

	public AiStateId GetId()
	{
		return AiStateId.Destroy;
	}

	public void Update(AIAgent agent)
	{
		agent.ragdoll.ActivateRagdoll();
		//direction.y = 1;
		agent.ragdoll.ApplyForce(direction * agent.aIAgentConfig.dieForce);
	}
}
