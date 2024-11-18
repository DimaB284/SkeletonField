using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 10f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // ������ CaptureZone, � ��� ���������� �����
        CaptureZone captureZone = FindCaptureZone();

        if (captureZone != null)
        {
            // ��������� ����� ��� ��������� ������
            captureZone.OnEntityDestroyed(transform, "Enemy");
        }

        // ������� ������
        Destroy(gameObject);
    }

    CaptureZone FindCaptureZone()
    {
        // ��������� ���� ���������� �� ��������� ������� ��� ���������
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // ����� ��������
        foreach (Collider collider in colliders)
        {
            CaptureZone zone = collider.GetComponent<CaptureZone>();
            if (zone != null)
            {
                return zone;
            }
        }
        return null; // �� ������� ����
    }
}
