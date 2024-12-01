using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEditor.Animations;

public class ActiveWeapon : MonoBehaviour
{
	public Transform crossHairTarget;
	public Transform weaponParent;
    RaycastWeapon weapon;
	public Animator anim;
	AnimatorOverrideController overrides;
	public Rig handIK;
	public Transform rightGroup;
	public Transform leftGroup;
    // Start is called before the first frame update
    void Start()
    {
		anim = GetComponent<Animator>();
		overrides = anim.runtimeAnimatorController as AnimatorOverrideController;
		RaycastWeapon existingWeapon = GetComponentInChildren<RaycastWeapon>();
		if (existingWeapon)
		{
			Equip(existingWeapon);
		}
    }

    // Update is called once per frame
    void Update()
    {
		if (weapon)
		{
			if (Input.GetButtonDown("Fire1"))
			{
				weapon.StartFiring();
			}
			if (weapon.isFiring)
			{
				weapon.UpdateFiring(Time.deltaTime);
			}
			weapon.UpdateBullets(Time.deltaTime);
			if (Input.GetButtonUp("Fire1"))
			{
				weapon.StopFiring();
			}
		}
		else
		{
			handIK.weight = 0.0f;
			anim.SetLayerWeight(1, 0.0f);
		}
		
	}

	public void Equip(RaycastWeapon newWeapon)
	{
		if (weapon)
		{
			Destroy(weapon.gameObject);
		}
		weapon = newWeapon;
		weapon.raycastDestination = crossHairTarget;
		weapon.transform.parent = weaponParent;
		weapon.transform.localPosition = Vector3.zero;
		weapon.transform.localRotation = Quaternion.identity;

		handIK.weight = 1.0f;
		anim.SetLayerWeight(1, 1.0f);

		Invoke(nameof(SetAnimationDelayed), 0.001f);
	}

	void SetAnimationDelayed()
	{
		overrides["weapon_anim_empty"] = weapon.weaponClip;
	}

	[ContextMenu("Save Weapon Pose")]
	public void SaveWeaponPose()
	{
		GameObjectRecorder recorder = new GameObjectRecorder(gameObject);
		recorder.BindComponentsOfType<Transform>(weaponParent.gameObject, false);
		recorder.BindComponentsOfType<Transform>(rightGroup.gameObject, false);
		recorder.BindComponentsOfType<Transform>(leftGroup.gameObject, false);
		recorder.TakeSnapshot(0.0f);
		recorder.SaveToClip(weapon.weaponClip);
		UnityEditor.AssetDatabase.SaveAssets();
	}
}
