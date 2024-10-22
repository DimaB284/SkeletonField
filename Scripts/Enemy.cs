using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public Vector3 walkPoint;
    public GameObject projectile;
    bool isWalkPointSet;
    [SerializeField] float walkPointRange;

    [SerializeField] float timeBtwAttacks;
    bool isAlreadyAttacked;
    [SerializeField] float sightRange, attackRange;
    public bool isPlayerInSightRange, isPlayerInAttackRange;
    void Awake() 
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        isPlayerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

       /* if (!isPlayerInSightRange && !isPlayerInAttackRange)
        {
            Patrolling();
        }

        if (isPlayerInSightRange && !isPlayerInAttackRange)
        {
            ChasePlayer();
        }

        if (isPlayerInSightRange && isPlayerInAttackRange)
        {
            AttackPlayer();
        }*/

        ChasePlayer();
        AttackPlayer();

        
    }

    void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            isWalkPointSet = true;
        }

    }

    void Patrolling()
    {
        if (!isWalkPointSet)
        {
            SearchWalkPoint();
        }

        if (isWalkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            isWalkPointSet = false;
        }

    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);
        if (!isAlreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            isAlreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBtwAttacks);
        }
    }

    void ResetAttack()
    {
        isAlreadyAttacked = false;
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

}
