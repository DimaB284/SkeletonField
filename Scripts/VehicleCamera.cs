using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
    [SerializeField] Transform[] povs;
    [SerializeField] float speed;
    private int index = 1;
    private Vector3 target;
    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            index = 0;
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            index = 1;
        }

        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            index = 2;
        }

        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            index = 3;
        }
        target = povs[index].position;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;
    }
   /* [SerializeField] GameObject thingToFollow;
    [SerializeField] float dimensionForX;
    [SerializeField] float dimensionForY;
    [SerializeField] float dimensionForZ;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = thingToFollow.transform.position + new Vector3(dimensionForX, dimensionForY, dimensionForZ);
    }*/
}
