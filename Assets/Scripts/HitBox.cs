using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public EnemyHealth enemyHealth;

    public void OnRaycastHit(Gun gun, Vector3 direction)
	{
        enemyHealth.TakeDamage(gun.damage, direction);
	}
}
