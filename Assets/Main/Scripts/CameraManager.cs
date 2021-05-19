using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float mainSpeed = 100.0f; //regular speed
    public float rotSpeed = 10f;

    public float velocityModifier = 1f;
    public float rotationModifier = 1f;

    void Update()
    {
        /*lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;*/
        //Mouse  camera angle done.  

        //Keyboard commands
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        {
            p = p * mainSpeed;
            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }

        Vector3 r = GetRotationInput();
        if (r.sqrMagnitude > 0)
        {
            r = r * rotSpeed * Time.deltaTime;

            // Get the flat forward vector by transform the eulers to (0, Y, 0)
            Vector3 eulers = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, eulers.y, 0);
            Vector3 forward_flat = Vector3.Normalize(transform.forward) * 10f;
            // Reset the eulers
            transform.eulerAngles = eulers;

            // Rotate the camera
            Vector3 point = RotatePointAroundPivot(transform.position, transform.position + forward_flat, r);
            transform.eulerAngles = eulers + r;
            transform.position = point;
        }
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.Z))
        {
            p_Velocity += new Vector3(0, 0, velocityModifier);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -velocityModifier);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            p_Velocity += new Vector3(-velocityModifier, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(velocityModifier, 0, 0);
        }
        return p_Velocity;
    }

    private Vector3 GetRotationInput()
    {
        Vector3 r_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.A))
        {
            r_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            r_Velocity += new Vector3(0, -1, 0);
        }
        return r_Velocity;
    }
}
