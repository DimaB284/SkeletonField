using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWeapons : MonoBehaviour
{
	public Transform gun;
	Gun currentGun;
	Transform currentTarget;
	WeaponIK weaponIK;
	// Start is called before the first frame update
	void Start()
    {
		weaponIK = GetComponent<WeaponIK>();
		currentGun = GetComponent<Gun>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentGun &&  currentTarget)
		{
			Vector3 target = currentTarget.position;
			//currentGun.
		}
    }
	public void SetFiring(bool enabled)
	{
		if (enabled)
		{
			//currentGun.Shoot();
		}
	}
	public void DropWeapon()
	{
		if (gun)
		{
			gun.transform.SetParent(null);
			gun.gameObject.GetComponent<BoxCollider>().enabled = true;
			gun.gameObject.AddComponent<Rigidbody>();
			StartCoroutine(DestroyAfterDelay(gun.gameObject));
		}
	}

   private IEnumerator DestroyAfterDelay(GameObject gun)
   {
	   float delay = 3f;
	   yield return new WaitForSeconds(delay);
	   Object.Destroy(gun.gameObject);
   }

	public void SetTarget (Transform target)
	{
		weaponIK.SetTargetTransform(target);
		currentTarget = target;
	}
}
