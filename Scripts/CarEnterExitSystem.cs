using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEnterExitSystem : MonoBehaviour
{

    [SerializeField] MonoBehaviour CarController;
    [SerializeField] Transform Car;
    [SerializeField] Transform Player;

    [Header("Cameras")]
    [SerializeField] GameObject PlayerCam;
    [SerializeField] GameObject CarCam;

    [SerializeField] GameObject DriveUi;

    bool canDrive;



    // Start is called before the first frame update
    void Start()
    {
        CarController.enabled = false;
        DriveUi.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.F) && canDrive)  // Here After Click F button and trigger is true player is driving
        {
           
            CarController.enabled = true; // After Click F button Car Controller Script is enabled

            DriveUi.gameObject.SetActive(false);

            // Here we parent Car with player
            Player.transform.SetParent(Car);
            Player.gameObject.SetActive(false);

            // Camera
            PlayerCam.gameObject.SetActive(false);
            CarCam.gameObject.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            CarController.enabled = false; // After Click G button Car Controller Script is disable

            // Here We Unparent the Player with Car
            Player.transform.SetParent(null);
            Player.gameObject.SetActive(true);
            //Player.transform.Rotate(0, 0, 0);

            // Here If Player Is Not Driving So PlayerCamera turn On and Car Camera turn off

            PlayerCam.gameObject.SetActive(true);
            CarCam.gameObject.SetActive(false);
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            DriveUi.gameObject.SetActive(true);
            canDrive = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            DriveUi.gameObject.SetActive(false);
            canDrive = false;
        }
    }
}