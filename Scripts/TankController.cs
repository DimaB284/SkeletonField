using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Швидкість руху танка
    public float turnSpeed = 60f; // Швидкість повороту корпусу

    [Header("Turret Settings")]
    public Transform turret; // Башта танка
    public float turretTurnSpeed = 100f; // Швидкість повороту башти

    [Header("Shooting Settings")]
    public Transform firePoint; // Точка стрільби
    public GameObject projectilePrefab; // Префаб снаряда
    public float fireForce = 20f; // Сила вистрілу
    public float fireCooldown = 1f; // Затримка між пострілами
    private bool canFire = true;

    void Update()
    {
        HandleMovement();
        HandleTurretRotation();
        HandleShooting();
    }

    // Управління рухом корпусу
    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical"); // Вперед/назад
        float turnInput = Input.GetAxis("Horizontal"); // Поворот корпусу

        // Рух вперед/назад
        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);

        // Поворот корпусу
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
    }

    // Управління поворотом башти
    private void HandleTurretRotation()
    {
        if (turret == null) return;

        // Поворот башти за напрямком миші
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPoint = hit.point;
            Vector3 direction = (targetPoint - turret.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            turret.rotation = Quaternion.RotateTowards(turret.rotation, lookRotation, turretTurnSpeed * Time.deltaTime);
        }
    }

    // Управління стрільбою
    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && canFire) // Ліва кнопка миші
        {
            StartCoroutine(FireProjectile());
        }
    }

    // Вистріл снарядом
    private IEnumerator FireProjectile()
    {
        canFire = false;

        // Створення снаряда
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(firePoint.forward * fireForce, ForceMode.Impulse);

        yield return new WaitForSeconds(fireCooldown);

        canFire = true;
    }
}
