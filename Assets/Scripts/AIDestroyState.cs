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
		   agent.ragdoll.ApplyForce(direction * agent.aIAgentConfig.dieForce);
		   if (agent.hasGun)
		   {
			   agent.gun.DropWeapon();
		   }
		   agent.weaponIK.enabled = false;
		   if (agent.navMeshAgent != null)
			   agent.navMeshAgent.enabled = false; // Вимикаємо NavMeshAgent після смерті

		   // Запускаємо корутину на зникнення тіла та зброї
		   agent.StartCoroutine(DestroyAfterDelay(agent));
	   }
   private IEnumerator DestroyAfterDelay(AIAgent agent)
   {
	   float delay = Random.Range(3f, 5f);
	   yield return new WaitForSeconds(delay);

	   // Якщо зброя ще є у сцені (наприклад, DropWeapon робить її окремим об'єктом)
	   /*if (agent.gun != null && agent.gun.gameObject != null)
	   {
		   GameObject gunObj = agent.gun.gameObject;
		   if (gunObj.transform.parent == null) // Переконаємось, що зброя вже від'єднана
		   {
			   Object.Destroy(gunObj);
		   }
	   }*/

	   // Перед знищенням відреєструвати юніта, щоб лічильник живих був коректним
	   if (UnitManager.Instance != null)
	   {
	       UnitManager.Instance.UnregisterAgent(agent);
	   }
	   // Знищуємо тіло юніта
	   Object.Destroy(agent.gameObject);
	   //RespawnManager.Instance.AddToRespawnQueue(agent.team, 5f);
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
