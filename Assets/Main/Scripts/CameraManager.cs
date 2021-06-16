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
    FreeOverview,
    FirstPerson,
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

    /*private Vector3 OverPos;
    private Vector3 OverRot;

    private Vector3 FirstPPos;
    private Vector3 FirstPRot;*/

    public Camera firstPersonCamera;

    public float mouseSensitivity = 750f;

    void Update()
    {
        HandleCommands();

        if (camMode == cameraMode.FreeOverview || camMode == cameraMode.Overview)
            Overview();
    }

    private void Overview()
    {
        //MouseOverlayTest();
        // Should only work in Overview mode
        if (overlay && _operator)
            MouseOverlay();

        // Lookat
        if (camMode == cameraMode.FreeOverview)
        {
            lastMouse = new Vector3(-Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime, Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 0);
            lastMouse.x = Mathf.Clamp(lastMouse.x, -90f, 90f);
            lastMouse.y = Mathf.Clamp(lastMouse.y, -90f, 90f);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
        }
        if (camMode == cameraMode.Overview)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                this.transform.eulerAngles = new Vector3(28.84f, 0, 0);
                this.transform.position = new Vector3(this.transform.position.x, 4f, this.transform.position.z);
            }
        }

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
        cameraMode oldMode = camMode;
        bool modeChanged = false;

        if ((oldMode == cameraMode.FreeOverview || oldMode == cameraMode.FreeFirstPerson) && Input.GetKeyDown(KeyCode.Escape))
        {
            camMode = oldMode == cameraMode.FreeOverview ? cameraMode.Overview : cameraMode.FirstPerson;
            modeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            camMode = cameraMode.Overview;
            modeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            camMode = cameraMode.FreeOverview;
            modeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Keypad2) && Manager.Instance.characters.Count > 0)
        {
            camMode = cameraMode.FirstPerson;
            modeChanged = true;
        }
        if (Input.GetKeyDown(KeyCode.Keypad3) && Manager.Instance.characters.Count > 0)
        {
            camMode = cameraMode.FreeFirstPerson;
            modeChanged = true;
        }

        /*
         * Not all cases are interesting.
         * We won't care about the change between Overview and FreeOverview which use the same camera.
         * We will only care about first person change (FP and FFP) and FP/FFP to Overview.
         */
        if (modeChanged)
        {
            if (oldMode <= cameraMode.FreeOverview && camMode >= cameraMode.FirstPerson)
            {
                // Select first person cam.
                // Disable main cam et enable sub cam
                GameObject character = Manager.Instance.GetNextFirstPerson();
                if (character)
                {
                    Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                    cam.enabled = true;
                    Camera.main.enabled = false;
                    character.GetComponent<CharacterManager>().TogglePlayer();
                }
            }
            else if (oldMode > cameraMode.FreeOverview && camMode >= cameraMode.FirstPerson)
            {
                if (oldMode == camMode)
                {
                    // Select new sub camera is available (meaning more than one character present)
                    GameObject character = Manager.Instance.GetCurrentFirstPerson();
                    GameObject nextCharacter = Manager.Instance.GetNextFirstPerson();
                    if (character && nextCharacter && character != nextCharacter)
                    {
                        Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                        cam.enabled = false;
                        character.GetComponent<CharacterManager>().TogglePlayer();
                        cam = nextCharacter.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                        cam.enabled = true;
                        nextCharacter.GetComponent<CharacterManager>().TogglePlayer();
                    }
                }
                // The other case is a simple update which tell the characterManager if the player can control the character and not simply being in the player 
            }
            else if (oldMode > cameraMode.FreeOverview && camMode <= cameraMode.FreeOverview)
            {
                // Enable main cam and disable sub cam
                GameObject character = Manager.Instance.GetCurrentFirstPerson();
                if (character)
                {
                    Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                    cam.enabled = false;
                    character.GetComponent<CharacterManager>().TogglePlayer();
                }
                Manager.Instance.camKaren.GetComponent<Camera>().enabled = true;
            }
        }


        // Hide cursor when camera mode is not overview.
        if (camMode == cameraMode.Overview)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
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

    public void ChangeView(int view)
    {
        cameraMode oldMode = camMode;
        bool modeChanged = false;

        if (view == 0)
        {
            camMode = cameraMode.Overview;
            modeChanged = true;
        }
        if (view == 1)
        {
            camMode = cameraMode.FreeOverview;
            modeChanged = true;
        }
        if (view == 2 && Manager.Instance.characters.Count > 0)
        {
            camMode = cameraMode.FirstPerson;
            modeChanged = true;
        }
        if (view == 3 && Manager.Instance.characters.Count > 0)
        {
            camMode = cameraMode.FreeFirstPerson;
            modeChanged = true;
        }

        /*
         * Not all cases are interesting.
         * We won't care about the change between Overview and FreeOverview which use the same camera.
         * We will only care about first person change (FP and FFP) and FP/FFP to Overview.
         */
        if (modeChanged)
        {
            if (oldMode <= cameraMode.FreeOverview && camMode >= cameraMode.FirstPerson)
            {
                // Select first person cam.
                // Disable main cam et enable sub cam
                GameObject character = Manager.Instance.GetNextFirstPerson();
                if (character)
                {
                    Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                    cam.enabled = true;
                    Camera.main.enabled = false;
                    character.GetComponent<CharacterManager>().TogglePlayer();
                }
            }
            else if (oldMode > cameraMode.FreeOverview && camMode >= cameraMode.FirstPerson)
            {
                if (oldMode == camMode)
                {
                    // Select new sub camera is available (meaning more than one character present)
                    GameObject character = Manager.Instance.GetCurrentFirstPerson();
                    GameObject nextCharacter = Manager.Instance.GetNextFirstPerson();
                    if (character && nextCharacter && character != nextCharacter)
                    {
                        Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                        cam.enabled = false;
                        character.GetComponent<CharacterManager>().TogglePlayer();
                        cam = nextCharacter.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                        cam.enabled = true;
                        nextCharacter.GetComponent<CharacterManager>().TogglePlayer();
                    }
                }
                // The other case is a simple update which tell the characterManager if the player can control the character and not simply being in the player 
            }
            else if (oldMode > cameraMode.FreeOverview && camMode <= cameraMode.FreeOverview)
            {
                // Enable main cam and disable sub cam
                GameObject character = Manager.Instance.GetCurrentFirstPerson();
                if (character)
                {
                    Camera cam = character.GetComponent<CharacterManager>().cam.GetComponent<Camera>();
                    cam.enabled = false;
                    character.GetComponent<CharacterManager>().TogglePlayer();
                }
                Manager.Instance.camKaren.GetComponent<Camera>().enabled = true;
            }
        }


        // Hide cursor when camera mode is not overview.
        if (camMode == cameraMode.Overview)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        EventSystem.current.SetSelectedGameObject(null);
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
                if (_operator.GetComponent<BoxCollider>())
                    _operator.GetComponent<BoxCollider>().enabled = true;
                if (_operator.GetComponent<CharacterController>())
                    _operator.GetComponent<CharacterController>().enabled = true;
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
    private void MouseOverlayTest()
    {
        Ray ray;
        RaycastHit hit;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            print(hit.collider);
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
