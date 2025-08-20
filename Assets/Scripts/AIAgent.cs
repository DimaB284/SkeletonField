using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    public AIStateMachine stateMachine;
    public AiStateId initialState;
    public NavMeshAgent navMeshAgent;
    public AIAgentConfig aIAgentConfig;
    public Ragdoll ragdoll;
    public Transform player;
    public bool hasGun;
    public AIWeapons gun;
	public MonoBehaviour weaponIK;

	public Animator animator; // Додано поле для Animator

	public GameObject bulletPrefab; //  
	public Transform firePoint; // ,   
    [SerializeField] ParticleSystem enemyMuzzleFlash;
    public AiStateId previousState; // Для повернення до попереднього стану після атаки

    public enum Faction { Enemy, Ally, Player }
    public Faction faction;

    public AISensor sensor; // Додаю посилання на AISensor

    public bool IsTargetInFOV(Transform target)
    {
        if (target == null) return false;
        Vector3 dir = (target.position - transform.position);
        float dist = dir.magnitude;
        float maxDistance = sensor != null ? sensor.distance : (aIAgentConfig != null ? aIAgentConfig.maxSightDistance : 30f);
        if (dist > maxDistance) {
            return false;
        }
        dir.y = 0;
        dir.Normalize();

        float yOffset = sensor != null ? sensor.yRotationOffset : 0f;
        Quaternion offsetRot = Quaternion.Euler(0, yOffset, 0);
        Vector3 fovForward = offsetRot * transform.forward;

        float angleLimit = sensor != null ? sensor.angle : 180f;
        float angleToTarget = Vector3.Angle(fovForward, dir);
        if (angleToTarget > angleLimit) {
            return false;
        }
        float maxHeight = sensor != null ? sensor.height : 5f;
        float heightDiff = Mathf.Abs(target.position.y - transform.position.y);
        if (heightDiff > maxHeight) {
            return false;
        }
        return true;
    }

    public void Shoot()
	{
		Debug.Log($"[AI] {name}: Shoot() called");
		if (bulletPrefab == null || firePoint == null) return;
		Debug.Log($"[AI] {name} SHOOTS at {firePoint.forward}");
		GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
		var bulletComp = bullet.GetComponent<Bullet>();
		if (bulletComp != null)
		{
			bulletComp.shooterFaction = this.faction;
		}
		// Уникнути колізій кулі зі своїм стрільцем (зброя/тіло)
		Collider bulletCol = bullet.GetComponent<Collider>();
		if (bulletCol != null)
		{
			Collider[] selfCols = GetComponentsInChildren<Collider>();
			foreach (var col in selfCols)
			{
				if (col != null && col.enabled)
				{
					Physics.IgnoreCollision(bulletCol, col, true);
				}
			}
		}
		Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (enemyMuzzleFlash != null)
        {
            enemyMuzzleFlash.Play();
        }

        if (bulletRb != null)
		{
			bulletRb.velocity = firePoint.forward * 20f; //   
		}
       // Debug.Log("Enemy Shoots!");
	}

    public Transform FindTarget()
    {
        AIAgent[] allAgents = FindObjectsOfType<AIAgent>();
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var agent in allAgents)
        {
            if (agent == this) continue;
            // Союзники шукають ворогів, вороги — гравця та союзників
            bool isEnemy = (this.faction == Faction.Ally && agent.faction == Faction.Enemy)
                        || (this.faction == Faction.Enemy && (agent.faction == Faction.Ally || agent.faction == Faction.Player));
            if (!isEnemy) continue;
            float dist = Vector3.Distance(transform.position, agent.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = agent.transform;
            }
        }
        // Якщо агент — союзник або ворог, і гравець є ворогом, враховуємо гравця
        if (this.faction == Faction.Ally || this.faction == Faction.Enemy)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && (this.faction == Faction.Enemy))
            {
                float dist = Vector3.Distance(transform.position, playerObj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = playerObj.transform;
                }
            }
        }
        return closest;
    }
	// Start is called before the first frame update
	void Start()
    {
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = 3.5f; // Зменшена швидкість ворога
            // Якщо агент не на NavMesh (щойно заспавнився на краю), спробувати прив'язати
            if (!navMeshAgent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    navMeshAgent.Warp(hit.position);
                }
            }
        }
        if (faction == Faction.Enemy)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        stateMachine = new AIStateMachine(this);
        stateMachine.RegisterState(new AIChasePlayerState());
        stateMachine.RegisterState(new AIDestroyState());
        stateMachine.RegisterState(new AIIdleState());
        stateMachine.RegisterState(new AIAttackPlayerState());
		stateMachine.RegisterState(new AIShootingState());
        stateMachine.RegisterState(new AIPatrolState()); // новий стан
        stateMachine.RegisterState(new AICaptureState()); // новий стан
        stateMachine.RegisterState(new AIZoneControlState()); // новий стан для перебування у зоні
		stateMachine.ChangeState(initialState);
		if (player == null)
		{
			stateMachine.ChangeState(AiStateId.Idle); // ��� ����-���� ����� ����
			return;
		}
	}

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"[AI] {name}: AIAgent.Update");
        stateMachine.Update();
        // Оновлення анімації руху
        if (animator != null && navMeshAgent != null)
        {
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        }
    }
}
