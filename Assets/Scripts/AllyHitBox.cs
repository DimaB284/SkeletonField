using UnityEngine;

public class AllyHitBox : MonoBehaviour
{
    public AllyHealth allyHealth;

    public void OnRaycastHit(Gun gun, Vector3 direction)
    {
        if (allyHealth != null)
        {
            allyHealth.TakeDamage(gun.damage, direction);
        }
    }
}


