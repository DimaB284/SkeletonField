using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour
{
    private bool isScoped = false;
    public WeaponSwitcher weaponSwitcher;
    public GameObject scopeOverlay;
    public GameObject weaponCamera;
    public Camera mainCamera;
    public float scopedFOV = 15f;
    private float normalFOV;
    // Start is called before the first frame update
    void Start()
    {
        weaponSwitcher = GetComponent<WeaponSwitcher>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2") && weaponSwitcher.selectedWeaponNumber == 3)
		{
            isScoped = !isScoped;
            scopeOverlay.SetActive(isScoped);
            if (isScoped)
			{
                StartCoroutine(OnScoped());
			}
			else
			{
                OnUnscoped();
			}
		}
        if (weaponSwitcher.selectedWeaponNumber != 3)
		{
            isScoped = false;
            scopeOverlay.SetActive(false);
            weaponCamera.SetActive(true);
            mainCamera.fieldOfView = 60;
        }
        
    }


    IEnumerator OnScoped()
	{
        yield return new WaitForSeconds(.15f);
        scopeOverlay.SetActive(true);
        weaponCamera.SetActive(false);
        normalFOV = mainCamera.fieldOfView;
        mainCamera.fieldOfView = scopedFOV;
	}



    void OnUnscoped()
	{
        scopeOverlay.SetActive(false);
        weaponCamera.SetActive(true);
        mainCamera.fieldOfView = normalFOV;
    }
}
