using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Tank Settings")]
    [SerializeField] Transform tankTower;
    [SerializeField] float towerRotateSpeed;
    [SerializeField] float steerSpeed;
    [SerializeField] float moveSpeed;
    // Start is called before the first frame update

    [Header("Shooting Settings")]
    [SerializeField] Transform cannon; // ������� ������� ��� �������
    [SerializeField] GameObject projectilePrefab; // ������
    [SerializeField] float fireForce = 500f; // ���� �������
    [SerializeField] float fireCooldown = 1f; // ��� �� ���������
    private float fireCooldownTimer = 0f;

    [Header("Tracks Animation")]
    [SerializeField] Transform leftTrack; // ˳�� ��������
    [SerializeField] Transform rightTrack; // ����� ��������
    [SerializeField] float trackSpeedMultiplier = 10f; // �������� 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleShooting();
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

    }

    private void FixedUpdate()
	{
        RotateTower();
        MoveInput();
        //AnimateTracks();
    }

    void RotateTower()
	{
        if (Input.GetKey(KeyCode.Q))
        {
            tankTower.Rotate(0f, -towerRotateSpeed, 0f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            tankTower.Rotate(0f, towerRotateSpeed, 0f);
        }
    }

    void MoveInput()
	{
        float steerAmount = Input.GetAxis("Horizontal") * steerSpeed * Time.deltaTime;
        float moveAmount = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Rotate(0, steerAmount, 0);
        transform.Translate(0, 0, moveAmount);
    }
    void AnimateTracks()
    {
        // ��������� �������� ������� �� �������� ����
        float moveAmount = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime * trackSpeedMultiplier;
        float steerAmount = Input.GetAxis("Horizontal") * steerSpeed * Time.deltaTime * trackSpeedMultiplier;

        if (leftTrack != null)
        {
            leftTrack.Rotate(moveAmount + steerAmount, 0, 0);
        }

        if (rightTrack != null)
        {
            rightTrack.Rotate(moveAmount - steerAmount, 0, 0);
        }
    }
    void HandleShooting()
    {
        // ��������, �� ��������� ������ ������� � �� ������� ���� �������
        if (Input.GetKeyDown(KeyCode.Space) && fireCooldownTimer <= 0)
        {
            Shoot();
            fireCooldownTimer = fireCooldown;
        }
    }

    void Shoot()
    {
        // ��������� �������
        if (cannon != null && projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, cannon.position, cannon.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(cannon.forward * fireForce, ForceMode.Impulse);
            }
        }
    }


}
