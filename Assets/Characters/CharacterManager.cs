using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{

    public Animator animManager;
    public bool inControl = false;
    public bool timeControl = false;

    public GameObject cam;
    public GameObject model;
    public CharacterController controller;

    public float speed = 1f;
    public float mouseSensitivity = 100f;

    float xRotation = 0.0f;

    private void Start()
    {
    }

    public void TogglePlayer()
    {
        inControl = !inControl;
        if (inControl)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void resetAnim()
    {
        animManager.SetBool("Walk", false);
        animManager.SetBool("Use", false);

        model.transform.localPosition = new Vector3(0, 0 ,0);
        model.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // If condition to remove (only used for debug)
        //if (/*Input.GetKeyDown(KeyCode.M) && */!Manager.Instance.characters.Contains(this.gameObject))
        /*{
            Manager.Instance.characters.Add(this.gameObject);
        }*/
        if (!timeControl)
            playerControl();
        else
        {

        }
        
    }

    private void playerControl()
    {
        if (inControl && Manager.Instance.camKaren.camMode == cameraMode.FreeFirstPerson)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            Vector3 trueSpeed = speed * Time.deltaTime * move;
            controller.Move(speed * Time.deltaTime * move);
            //this.transform.position = controller.transform.position; // Not crazy

            if (trueSpeed.sqrMagnitude > 0.0f)
                animManager.SetBool("Walk", true);
            else
                animManager.SetBool("Walk", false);
        }
        else if (inControl)
            animManager.SetBool("Walk", false); // Won't stay like this. Timeline action will take over
    }

}
