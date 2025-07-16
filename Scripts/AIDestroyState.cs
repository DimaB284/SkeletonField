using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDestroyState : AIState
{
	public Vector3 direction;
	//[SerializeField] MonoBehaviour weaponIK;

	public void Enter(AIAgent agent)
	{
		agent.ragdoll.ActivateRagdoll();
		//direction.y = 1;
		agent.ragdoll.ApplyForce(direction * agent.aIAgentConfig.dieForce);
		if (agent.hasGun)
		{
			agent.gun.DropWeapon();
		}
		agent.weaponIK.enabled = false;
		if (agent.navMeshAgent != null)
			agent.navMeshAgent.enabled = false; // Вимикаємо NavMeshAgent після смерті
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

	}

}
