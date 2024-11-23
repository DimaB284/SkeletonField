using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

	void Update()
	{
        if (agent.hasPath)
		{
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
		else
		{
            animator.SetFloat("Speed", 0);
		}
      
	}

	/*public Transform player;                // Посилання на гравця
    public GameObject bulletPrefab;         // Префаб кулі
    public Transform firePoint;             // Точка, з якої ворог стріляє
    public float detectionRange = 15.0f;    // Дистанція, з якої ворог починає переслідування
    public float shootingRange = 10.0f;     // Дистанція, з якої ворог починає стріляти
    public float fireRate = 1.0f;           // Частота стрільби (кулі в секунду)
    public float moveSpeed = 3.5f;          // Швидкість пересування ворога
    public float runSpeed = 6.0f;           // Швидкість бігу ворога
    public float bulletSpeed = 20.0f;       // Швидкість польоту кулі
    public float attackCooldown = 1.5f;     // Час між атаками

    private NavMeshAgent agent;             // Компонент NavMeshAgent для навігації
    private float lastShotTime = 0f;        // Час останньої стрільби
    private bool isRunning = false;         // Чи ворог біжить

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;            // Встановити стандартну швидкість пересування
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= shootingRange)
            {
                // Зупинитися і стріляти
                agent.isStopped = true;
                //agent.SetDestination(player.position);
                transform.LookAt(player);
                if (Time.time > lastShotTime + 1f / fireRate)
                {
                    Shoot();
                    lastShotTime = Time.time;
                }
            }
            else
            {
                // Переслідувати гравця
                agent.isStopped = false;
                agent.SetDestination(player.position);

                // Перевірка, чи варто бігти
                if (!isRunning && distanceToPlayer > shootingRange)
                {
                    agent.speed = runSpeed;
                    isRunning = true;
                }
                else if (isRunning && distanceToPlayer <= shootingRange)
                {
                    agent.speed = moveSpeed;
                    isRunning = false;
                }
            }
        }
        else
        {
            // Ворог зупиняється, якщо гравець поза зоною виявлення
            agent.isStopped = true;
        }
    }

    void Shoot()
    {
        // Створити кулю
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Додати швидкість кулі
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePoint.forward * bulletSpeed;

        // Тут можна додати додаткову логіку для стрільби (звук, анімації і т.д.)
        Debug.Log("Enemy shoots!");
    }*/
}
