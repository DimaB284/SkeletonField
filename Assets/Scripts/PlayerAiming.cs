using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAiming : MonoBehaviour
{
    public float turnSpeed = 15f;
    public Rig aimLayer;
	public float aimDuration = 0.3f;

	Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

	// Update is called once per frame
	private void FixedUpdate()
	{
        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yawCamera, 0), turnSpeed * Time.fixedDeltaTime);
	}
	private void LateUpdate()
	{
		if (aimLayer)
		{
			/*if (Input.GetButton("Fire2"))
			{
				aimLayer.weight += Time.deltaTime / aimDuration;
			}
			else
			{
				aimLayer.weight -= Time.deltaTime / aimDuration;
			}*/
			aimLayer.weight = 1.0f;
		}
	}
}
