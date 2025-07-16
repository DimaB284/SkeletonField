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
        Debug.Log($"[AI] {name}: IsTargetInFOV called for {target?.name}");
        if (sensor == null || target == null) return false;
        Vector3 dir = (target.position - transform.position);
        float dist = dir.magnitude;
        Debug.Log($"[AI] {name}: Distance to {target.name}: {dist}, sensor.distance: {sensor.distance}");
        if (dist > sensor.distance) {
            Debug.Log($"[AI] {name}: Target too far");
            return false;
        }
        dir.y = 0;
        dir.Normalize();

        Quaternion offsetRot = Quaternion.Euler(0, sensor.yRotationOffset, 0);
        Vector3 fovForward = offsetRot * transform.forward;

        float angleToTarget = Vector3.Angle(fovForward, dir);
        Debug.Log($"[AI] {name}: Angle to {target.name}: {angleToTarget}, sensor.angle: {sensor.angle}");
        if (angleToTarget > sensor.angle) {
            Debug.Log($"[AI] {name}: Target out of angle");
            return false;
        }
        float heightDiff = Mathf.Abs(target.position.y - transform.position.y);
        Debug.Log($"[AI] {name}: Height diff to {target.name}: {heightDiff}, sensor.height: {sensor.height}");
        if (heightDiff > sensor.height) {
            Debug.Log($"[AI] {name}: Target out of height");
            return false;
        }
        Debug.Log($"[AI] {name}: Target {target.name} in FOV! Angle: {angleToTarget}, Dist: {dist}, HeightDiff: {heightDiff}");
        return true;
    }

    public void Shoot()
	{
		Debug.Log($"[AI] {name}: Shoot() called");
		if (bulletPrefab == null || firePoint == null) return;
		Debug.Log($"[AI] {name} SHOOTS at {firePoint.forward}");
		GameObject bullet = GameObject.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
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
        navMeshAgent.speed = 3.5f; // Зменшена швидкість ворога
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
        if (player != null)
        {
            stateMachine.Update();
        }
        // Оновлення анімації руху
        if (animator != null && navMeshAgent != null)
        {
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        }
    }
}
