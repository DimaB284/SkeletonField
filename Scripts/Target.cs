using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

    [SerializeField] float health;
    [SerializeField] bool isBarrel;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] float radius = 20f;
    [SerializeField] float force = 300f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            if (isBarrel)
            {
                Instantiate(explosionEffect, transform.position, transform.rotation);
                Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
                foreach (Collider nearbyObject in colliders)
                {
                     Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                     if (rb != null)
                     {
                          rb.AddExplosionForce(force, transform.position, radius);
                     }
                }
                Die();
            }
            else
            {
                Die();
            }
        }

    }
    void Die()
    {
        Destroy(gameObject);
    }
}
