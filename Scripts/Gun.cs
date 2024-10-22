using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float range;
    [SerializeField] float impactForce;
    [SerializeField] float fireRate;
    [SerializeField] float reloadTime;

    private float nextTimeToFire = 0f;

    public Animator animator;

    [SerializeField] int maxAmmo;
    private int currentAmmo;
    private bool isReloading = false;

    [SerializeField] Camera fpsCam;
    [SerializeField] ParticleSystem muzzleFlash;

    void Start() 
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);

    }

    void Update()
    {
        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if(Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f/fireRate;
            Shoot();
        }
    }
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        animator.SetBool("Reloading", true);
        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.25f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    void Shoot()
    {
        muzzleFlash.Play();
        currentAmmo--;
        RaycastHit raycastHit;
        if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out raycastHit, range))
        {
            Debug.Log(raycastHit.transform.name);
            Target target = raycastHit.transform.GetComponent<Target>();
            if(target != null)
            {
                target.TakeDamage(damage);
            }
             if(raycastHit.rigidbody != null)
            {
                raycastHit.rigidbody.AddForce(-raycastHit.normal * impactForce);
            }
        }


    }
}
