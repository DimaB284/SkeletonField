using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    //[SerializeField] Rigidbody rb;
    //[SerializeField] FixedJoystick joystick;
    [SerializeField] float steerSpeed;
    [SerializeField] float moveSpeed;
    public bool isKamAZ;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isKamAZ)
        {
            float steerAmount = Input.GetAxis("Horizontal") * steerSpeed * Time.deltaTime;
            float moveAmount = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            transform.Rotate(0, 0, steerAmount);
            transform.Translate(0, -moveAmount, 0);
        }
        else
        {
            float steerAmount = Input.GetAxis("Horizontal") * steerSpeed * Time.deltaTime;
            float moveAmount = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            transform.Rotate(0, steerAmount, 0);
            transform.Translate(0, 0, moveAmount);
        }


    }
    /*void FixedUpdate() 
    {
        rb.velocity = new Vector3(joystick.Horizontal * moveSpeed, rb.velocity.y, joystick.Vertical * moveSpeed);
    }*/
}

