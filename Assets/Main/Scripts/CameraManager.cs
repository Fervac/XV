using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum overlayType
{
    MOVE,
    TAKE,
    PUT,
    MOUNT
}

public enum cameraMode
{
    Overview,
    FirstPerson,
    FreeOverview,
    FreeFirstPerson
}

public class CameraManager : MonoBehaviour
{
    public float mainSpeed = 100.0f; //regular speed
    public float rotSpeed = 10f;

    public float velocityModifier = 1f;
    public float rotationModifier = 1f;

    public bool overlay = false;
    public overlayType overlay_type = 0;
    public GameObject _operator = null;

    private float overlayTimer = 0.0f;
    private int overlayTimerModifier = 1;

    public cameraMode camMode = cameraMode.Overview;

    private Vector3 lastMouse = new Vector3();
    private float camSens = 0.250f;

    private Vector3 OverPos;
    private Vector3 OverRot;

    private Vector3 FirstPPos;
    private Vector3 FirstPRot;

    void Update()
    {
        HandleCommands();

        // Should only work in Overview mode
        if (overlay && _operator)
            MouseOverlay();

        // Lookat
        if (camMode == cameraMode.FreeOverview || camMode == cameraMode.FreeFirstPerson)
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
        }
        else
            lastMouse = Input.mousePosition;

        // Move commands
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        {
            p = p * mainSpeed;
            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            transform.Translate(p);
            newPosition.x = transform.position.x;
            if (camMode == cameraMode.FreeOverview)
                newPosition.y = transform.position.y;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }

        // Rotation commands
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


    private void HandleCommands()
    {
        if (camMode != cameraMode.Overview && camMode != cameraMode.FreeOverview)
        {
            FirstPPos = this.transform.position;
            FirstPRot = this.transform.eulerAngles;
        }
        else
        {
            OverPos = this.transform.position;
            OverRot = this.transform.eulerAngles;
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
            camMode = cameraMode.Overview;
        if (Input.GetKeyDown(KeyCode.Keypad1))
            camMode = cameraMode.FreeOverview;
        if (Input.GetKeyDown(KeyCode.Keypad2))
            camMode = cameraMode.FirstPerson;
        if (Input.GetKeyDown(KeyCode.Keypad3))
            camMode = cameraMode.FreeFirstPerson;


        // Hide cursor when camera mode is not overview.
        if (camMode == cameraMode.Overview)
        {
            /*this.transform.position = OverPos;
            this.transform.eulerAngles = OverRot;*/
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            /*this.transform.position = FirstPPos;
            this.transform.eulerAngles = FirstPRot;*/
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Allow user to move timeline using scroll button
        if (camMode != cameraMode.Overview && Input.mouseScrollDelta.y != 0.0f)
        {
            float cursor = Manager.Instance.GetTimeCursor();
            float mod = Input.mouseScrollDelta.y >= 0.0f ? 1 : -1;

            Manager.Instance.timeline.UpdateCursor((cursor + 0.05f * mod) / Manager.Instance.GetDuration());
        }
    }
    private void MouseOverlay()
    {
        Vector3 endpoint = new Vector3(0, 0, 0);
        Ray ray;
        bool valid = false;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {

            endpoint = hit.point;
            if (hit.collider.CompareTag("Floor") && !(overlay_type == overlayType.MOUNT || overlay_type == overlayType.TAKE))
                valid = true;
            else if (!(hit.collider.CompareTag("Floor")) && hit.collider.gameObject != null && hit.collider.gameObject != _operator && (overlay_type == overlayType.MOUNT || overlay_type == overlayType.TAKE || overlay_type == overlayType.PUT))
                valid = true;
            else
                valid = false;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                overlay = false;
                Manager.Instance.TogglePopUp(true, true);
                _operator.GetComponent<BoxCollider>().enabled = true;
                _operator = null;
                valid = false;
                overlayTimer = 0.0f;
            }
            // Quit
            if (Input.GetMouseButtonDown(0) && valid)
            {
                _operator.GetComponent<PopupObjectMenu>().endpoint = endpoint;
                if (overlay_type == overlayType.MOUNT)
                    _operator.GetComponent<PopupObjectMenu>().mountTarget = hit.collider.gameObject;
                else if (overlay_type == overlayType.TAKE || overlay_type == overlayType.PUT)
                    _operator.GetComponent<PopupObjectMenu>().takeTarget = hit.collider.gameObject;
                overlay = false;
                _operator = null;
            }
        }

        if (overlayTimer < 0.0f)
            overlayTimer = 0.0f;
        else if (overlayTimer >= 0.5f)
            overlayTimer = 0.5f;
        GameObject.Find("MoveOverlay").transform.localScale = Vector3.Lerp(new Vector3(0.125f, 0.125f, 0.125f), new Vector3(0.0625f, 0.0625f, 0.0625f), overlayTimer / 0.5f);

        if (!overlay && GameObject.Find("MoveOverlay").activeSelf)
            GameObject.Find("MoveOverlay").transform.position = new Vector3(0, 0, 0);
        else
            GameObject.Find("MoveOverlay").transform.position = endpoint + new Vector3(0, 0.01f, 0);
        overlayTimer += Time.deltaTime * overlayTimerModifier;
        if (overlayTimer <= 0.0f)
            overlayTimerModifier = 1;
        else if (overlayTimer > 0.5f)
            overlayTimerModifier = -1;
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
        if (EventSystem.current.currentSelectedGameObject == null)
        {
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
            if (Input.GetKey(KeyCode.LeftShift) && camMode == cameraMode.FreeOverview)
            {
                p_Velocity += new Vector3(0, -velocityModifier, 0);
            }
            if (Input.GetKey(KeyCode.LeftControl) && camMode == cameraMode.FreeOverview)
            {
                p_Velocity += new Vector3(0, velocityModifier, 0);
            }
        }
        return p_Velocity;
    }

    private Vector3 GetRotationInput()
    {
        Vector3 r_Velocity = new Vector3();
        if (EventSystem.current.currentSelectedGameObject == null)
            {
            if (Input.GetKey(KeyCode.A))
            {
                r_Velocity += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(KeyCode.E))
            {
                r_Velocity += new Vector3(0, -1, 0);
            }
        }
        return r_Velocity;
    }
}
