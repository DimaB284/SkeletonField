using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AircraftController : MonoBehaviour
{
    [SerializeField] float throttleIncrement = 10f;
    [SerializeField] float maxThrust = 200f;
    [SerializeField] float responsiveness = 100f;
    [SerializeField] float lift = 500f;


    private float throttle;
    private float horizontal;
    private float vertical;
    private float yaw;
    private float responseModifier{get { return (rb.mass / 10f) * responsiveness; }}


    Rigidbody rb;
    [SerializeField] TextMeshProUGUI hud;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }

    void HandleInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            throttle -= throttleIncrement;
        }
        throttle = Mathf.Clamp(throttle, 0f, 100f); 
    }

    void Update() 
    {
        HandleInputs();
        UpdateHUD();
    }

    void FixedUpdate() 
    {
        rb.AddForce(-transform.right * maxThrust * throttle);

        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(-transform.forward * vertical * responseModifier);
        rb.AddTorque(transform.right * horizontal * responseModifier);

        rb.AddForce(Vector3.up * rb.velocity.magnitude * lift);
    }

    void UpdateHUD()
    {
        hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        hud.text += "Altitude: " + transform.position.y.ToString("F0") + "m\n";
    }
}
