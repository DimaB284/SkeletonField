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

    public void Shoot()
	{
		if (bulletPrefab == null || firePoint == null) return;

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
	// Start is called before the first frame update
	void Start()
    {
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = 3.5f; // Зменшена швидкість ворога
        player = GameObject.FindGameObjectWithTag("Player").transform;

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
