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
    private bool angleSet = false;
    private float angle = 0.0f;

    private float takeDelta = 0.0f;

    void Start()
    {
        isMoving = false;
        isRotating = false;
        isTaking = false;
        isUsing = false;
        angleSet = false;

        current = null;
        mount = null;
        items = new List<GameObject>();
        SetCorrectOrientation();
    }

    public void ResetVariables()
    {
        isRotating = false;
        isTaking = false;
        isUsing = false;

        isMoving = false;
        angleSet = false;
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

        BoxCollider box = this.GetComponent<BoxCollider>();
        Vector3 nsize = Quaternion.AngleAxis(90, Vector3.up) * box.size;
        nsize.x = Mathf.Abs(nsize.x);
        nsize.y = Mathf.Abs(nsize.y);
        nsize.z = Mathf.Abs(nsize.z);
        box.size = nsize;
        box.center = new Vector3(0, bounds.extents.y, bounds.extents.z);
        // Reactivate box collider
        box.enabled = true;
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

    private bool CompleteAction(Action action)
    {
        if (action.start > Manager.Instance.GetTimeCursor())
            return true;
        if (action.end < Manager.Instance.GetTimeCursor())
        {
            switch(action.type)
            {
                case actionType.MOVE:
                    this.transform.position = action.end_pos;
                    break;
                default:
                    break;
            }
            return true;
        }
        return false;
    }

    /*
     * Function used to dispatch action behavior. 
     */
    public void PlayAction(Action action)
    {
        if (action != current)
            ResetVariables();
        current = action;
        if (action == null || CompleteAction(action))
            return;
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
                isMoving = true;
                isTaking = true;
                isUsing = false;
                break;
            case actionType.USE:
                isMoving = true;
                isUsing = true;
                isTaking = false;
                break;
            default:
                return;
        }
        if (isMoving)
            Move();
        if (isTaking)
            Take();
        if (isUsing)
            Use();
    }

    #region Animation Manager

    #endregion

    #region Animation Functions

    public void Move()
    {
        moveDelta = Manager.Instance.GetTimeCursor() - current.start;
        //Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
        /*if (moveDelta >= current.duration)
            moveDelta = current.duration;*/

        // Rotate object to face destination
        // First we compute the direction vector between the start point and the end point
        // The move will always end up facing the end point (after all when we walk we look toward the point we go to)
        if (!angleSet)
        {
            angle = current.angle;
            angleSet = true;
        }
        float angleDelta = Mathf.Lerp(0, angle, moveDelta / (0.15f));
        if (angleDelta != 0.0f || angleDelta != angle)
        {
            transform.eulerAngles = new Vector3(current.start_forward.x,
                current.start_forward.y + angleDelta,
                current.start_forward.z);
        }

        Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / (current.duration));
        // Animate model

        this.transform.position = position;
        // Move stuff (and rotate stuff ?)
        if (moveDelta >= current.end)
        {
            moveDelta = 0.0f;
            isMoving = false;
        }
    }

    public void Take()
    {
        takeDelta = Manager.Instance.GetTimeCursor() - current.start;
        // Animate model
        if (!items.Contains(current.object_target))
            items.Add(current.object_target);

        current.object_target.transform.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), takeDelta);
        // Take stuff
        if (takeDelta >= current.duration)
        {
            takeDelta = 0.0f;
            isTaking = false;
        }
    }

    public void Use()
    {
        // Use
    }
    #endregion

    void Update()
    {
        if (Manager.Instance.IsPlaying())
            PlayAction(current);
        /*else if (Manager.Instance.GetTimeCursor() != timePos)
            GoToAction();*/
        
        /* Little code to show where is the forward direction
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 2f;
        Debug.DrawRay(transform.position, forward, Color.green);
        */
    }
}