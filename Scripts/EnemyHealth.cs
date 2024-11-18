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
        // Знайти CaptureZone, в якій знаходився ворог
        CaptureZone captureZone = FindCaptureZone();

        if (captureZone != null)
        {
            // Викликати метод для оновлення списків
            captureZone.OnEntityDestroyed(transform, "Enemy");
        }

        // Знищити ворога
        Destroy(gameObject);
    }

    CaptureZone FindCaptureZone()
    {
        // Знаходимо зону захоплення за допомогою тригерів або близькості
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f); // Радіус перевірки
        foreach (Collider collider in colliders)
        {
            CaptureZone zone = collider.GetComponent<CaptureZone>();
            if (zone != null)
            {
                return zone;
            }
        }
        return null; // Не знайшли зону
    }
}
