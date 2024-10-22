using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float responsiveness = 500f;
    [SerializeField] private float throttleAmount = 25f;
    private float throttle;
    private float horizontal;
    private float vertical;
    private float yaw; 

    [SerializeField] private float rotorSpeedModifier = 10f;
    [SerializeField] float maxThrust = 5f;
    [SerializeField] private Transform rotorsTransform;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        HandleInputs();
        rotorsTransform.Rotate(Vector3.up * (maxThrust * throttle) * rotorSpeedModifier);
    }

    void FixedUpdate() 
    {
        rb.AddForce(transform.up * throttle, ForceMode.Impulse);

        rb.AddTorque(transform.up * yaw * responsiveness);
        rb.AddTorque(-transform.forward * horizontal * responsiveness );
        rb.AddTorque(transform.right * vertical * responsiveness);
    }


    void HandleInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += Time.deltaTime * throttleAmount;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            throttle -= Time.deltaTime * throttleAmount;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f); 
    }
}
