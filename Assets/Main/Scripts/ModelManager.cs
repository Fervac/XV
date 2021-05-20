using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour
{

    public List<GameObject> items;
    public GameObject mount;

    public bool isMoving { get; set; }
    public bool isRotating { get; set; }
    public bool isTaking { get; set; }
    public bool isUsing { get; set; }

    public Action current;

    private float moveDelta = 0.0f;
    private float takeDelta = 0.0f;

    private float timePos = 0.0f;

    void Start()
    {
        isMoving = false;
        isRotating = false;
        isTaking = false;
        isUsing = false;

        current = null;
        mount = null;
        items = new List<GameObject>();
        SetCorrectOrientation();
    }

    private void SetCorrectOrientation()
    {
        GameObject parent = new GameObject("ModelParts");
        List<Transform> children = new List<Transform>();

        parent.transform.SetParent(this.transform);
        parent.transform.localPosition = new Vector3(0, 0, 0);
        foreach (Transform child in transform)
            children.Add(child);
        foreach (Transform child in children)
            child.SetParent(parent.transform);
        children.Clear();

        this.transform.GetChild(0).localEulerAngles = new Vector3(0, 90 - this.transform.eulerAngles.y, 0);
        Bounds bounds = CalculateLocalBounds();
        this.transform.GetChild(0).localPosition = new Vector3(bounds.extents.x, this.transform.GetChild(0).localPosition.y, this.transform.GetChild(0).localPosition.z);
    }

    private Bounds CalculateLocalBounds()
    {
        Quaternion currentRotation = this.transform.rotation;
        this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds bounds = new Bounds(this.transform.position, Vector3.zero);

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - this.transform.position;
        bounds.center = localCenter;

        this.transform.rotation = currentRotation;
        return bounds;
    }

    /*
     * Function used to dispatch action behavior. 
     */
    public void DoAction(Action action)
    {
        switch (action.type)
        {
            case actionType.MOVE:
                isMoving = true;
                isTaking = false;
                break;
            case actionType.ROTATE:
                isRotating = true;
                break;
            case actionType.TAKE:
                isTaking = true;
                isUsing = false;
                break;
            case actionType.USE:
                isUsing = true;
                isTaking = false;
                break;
            default:
                return;
        }
        if (isMoving)
            Move();
    }

    #region Animation Manager

    /*
     * To play action, we need to find which action is played at the current time.
     * To do this, we'll iterate through the list of its actions. Hum... This actor does not have access to the said list.
     */
    private void PlayAction()
    {
        //print(current);
        if (current == null)
            return;
        DoAction(current);
    }

    private void GoToAction()
    {
        if (current == null)
            return;
        if (current.start > Manager.Instance.GetTimeCursor() || current.end < Manager.Instance.GetTimeCursor())
        {
            current = null;
            return;
        }
        DoAction(current);
    }

    #endregion

    #region Animation Functions
    public void Move()
    {
        if (Manager.Instance.GetTimeCursor() < timePos)
            moveDelta -= -Manager.Instance.GetTimeCursor() + timePos;
        else
            moveDelta += Manager.Instance.GetTimeCursor() - timePos;
        //Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
        /*if (moveDelta >= current.duration)
            moveDelta = current.duration;*/
        Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
        // Animate model

        this.transform.position = position;
        // Move stuff (and rotate stuff ?)
        if (moveDelta >= current.duration || moveDelta <= 0.0f)
        {
            moveDelta = 0.0f;
            isMoving = false;
        }
        timePos = Manager.Instance.GetTimeCursor();
    }

    /*public void Rotate()
    {
        if (Manager.Instance.GetTimeCursor() < timePos)
            moveDelta -= -Manager.Instance.GetTimeCursor() + timePos;
        else
            moveDelta += Manager.Instance.GetTimeCursor() - timePos;
        //Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
        
        Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
        // Animate model

        this.transform.position = position;
        // Move stuff (and rotate stuff ?)
        if (moveDelta >= current.duration || moveDelta <= 0.0f)
        {
            moveDelta = 0.0f;
            isMoving = false;
        }
        timePos = Manager.Instance.GetTimeCursor();
    }*/

    public void Take()
    {
        takeDelta += Manager.Instance.GetTimeCursor() - timePos;
        // Animate model

        // Take stuff
        if (takeDelta >= current.duration)
        {
            items.Add(current.object_target);
            current.object_target.transform.localScale = new Vector3(0, 0, 0); // TODO : find other way to hide it | Maybe just `activeSelf = false`
            takeDelta = 0.0f;
            isTaking = false;
        }
        timePos = Manager.Instance.GetTimeCursor();
    }

    public void Use()
    {
        // Use
        timePos = Manager.Instance.GetTimeCursor();
    }
    #endregion

    void Update()
    {
        if (Manager.Instance.IsPlaying())
            PlayAction();
        else if (Manager.Instance.GetTimeCursor() != timePos)
            GoToAction();

        /* Little code to show where is the forward direction
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 2f;
        Debug.DrawRay(transform.position, forward, Color.green);
        */
    }
}