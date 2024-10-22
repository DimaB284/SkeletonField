using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public int selectedWeaponNumber = 0;
    // Start is called before the first frame update
    void Start()
    {
        SelectWeapon();
        
    }
    void Update()
    {
        int previousSelectedWeaponNumber = selectedWeaponNumber;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeaponNumber >= transform.childCount - 1)
            {
                selectedWeaponNumber = 0;
            }

            else
            {
                selectedWeaponNumber++;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeaponNumber <= 0)
            {
                selectedWeaponNumber = transform.childCount - 1;
            }

            else
            {
                selectedWeaponNumber--;
            }
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            selectedWeaponNumber = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2) && transform.childCount >= 2)
        {
            selectedWeaponNumber = 1;
        }

        if (Input.GetKey(KeyCode.Alpha3) && transform.childCount >= 3)
        {
            selectedWeaponNumber = 2;
        }

        if (Input.GetKey(KeyCode.Alpha4) && transform.childCount >= 4)
        {
            selectedWeaponNumber = 3;
        }

        if (previousSelectedWeaponNumber != selectedWeaponNumber)
        {
            SelectWeapon();
        }
        
    }

    void SelectWeapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selectedWeaponNumber)
            {
                weapon.gameObject.SetActive(true);
            }

            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }

    // Update is called once per frame
   
}
