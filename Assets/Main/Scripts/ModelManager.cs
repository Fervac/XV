using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour
{
    public List<GameObject> itemsEver = new List<GameObject>();
    public GameObject mountEver = null;

    public List<GameObject> items;
    private GameObject lastItem = null;
    public GameObject mount = null;

    public bool isMoving { get; set; }
    public bool isRotating { get; set; }
    public bool isTaking { get; set; }
    public bool isUsing { get; set; }

    public Action current;

    private float moveDelta = 0.0f;
    private bool angleSet = false;
    private float angle = 0.0f;

    private float takeDelta = 0.0f;
    private float useDelta = 0.0f;

    private float takeMoveDurationDenom = 5f;

    public Vector3 init_pos;
    public Vector3 init_rot;

    public GameObject init_parent;

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

        init_pos = this.transform.position;
        init_rot = this.transform.eulerAngles;
        init_parent = this.transform.parent.gameObject;
    }

    public void ResetVariables()
    {
        isRotating = false;
        isTaking = false;
        isUsing = false;

        isMoving = false;
        angleSet = false;
    }

    #region Setup (orientation and bounds) functions

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

    #endregion

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
                case actionType.TAKE:
                    this.transform.position = action.end_pos;
                    action.object_target.transform.localScale = new Vector3(0f, 0f, 0f);
                    break;
                case actionType.PUT:
                    this.transform.position = action.end_pos;
                    action.object_target.transform.localScale = new Vector3(1f, 1f, 1f);
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
                Move();
                break;
            case actionType.ROTATE:
                break;
            case actionType.TAKE:
                Take();
                break;
            case actionType.PUT:
                Put();
                break;
            case actionType.USE:
                Use();
                break;
            default:
                return;
        }
    }

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

    /*
     * Use to take an object from world to inventory
     */
    public void Take()
    {
        takeDelta = Manager.Instance.GetTimeCursor() - current.start;

        // The object should move till its near the target
        // then we should perform the "take" animation.
        // For the moment, we say that the take animation is one thenth of the action duration, the rest being the approch
        if (takeDelta < current.duration - (current.duration / takeMoveDurationDenom))
        {
            if (!Vector3.Equals(current.object_target.transform.localScale, new Vector3(1, 1, 1)))
            {
                current.object_target.transform.localScale = new Vector3(1, 1, 1);
                if (items.Contains(current.object_target))
                {
                    items.Remove(current.object_target);
                    current.object_target.transform.SetParent(GameObject.Find("Env/Objects").transform);
                }
            }
            if (!angleSet)
            {
                Vector3 dir = Vector3.Normalize(current.object_target.transform.position - transform.position);
                angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
                Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);
                angleSet = true;
            }
            float angleDelta = Mathf.Lerp(0, angle, takeDelta / (0.15f));
            if (angleDelta != 0.0f || angleDelta != angle)
            {
                transform.eulerAngles = new Vector3(current.start_forward.x,
                    current.start_forward.y + angleDelta,
                    current.start_forward.z);
            }

            Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, takeDelta / (current.duration - (current.duration / takeMoveDurationDenom)));
            this.transform.position = position;
        }
        else
        {
            // Current animation is to scale down the object we take.
            // Another one could be of putting the target onto the operator ?
            float scaleDelta = current.duration - takeDelta;
            current.object_target.transform.localScale = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), scaleDelta / ((current.duration / takeMoveDurationDenom)));
            if ((scaleDelta / ((current.duration / takeMoveDurationDenom))) <= 0.02f)
                if (!items.Contains(current.object_target))
                {
                    items.Add(current.object_target);
                    current.object_target.transform.SetParent(this.transform);
                    current.object_target.transform.position = this.transform.position;
                }
        }

        if (takeDelta >= current.duration)
        {
            takeDelta = 0.0f;
            isTaking = false;
        }
    }

    /*
     * Use to put an object in inventory somewhere in the world
     */
    public void Put()
    {
        takeDelta = Manager.Instance.GetTimeCursor() - current.start;

        if (takeDelta < current.duration - (current.duration / takeMoveDurationDenom))
        {
            if (items.Count < 1)
            {
                items.Add(lastItem);
                lastItem.transform.SetParent(this.transform);
            }
            if (!angleSet)
            {
                angle = current.angle;
                angleSet = true;
            }
            float angleDelta = Mathf.Lerp(0, angle, takeDelta / (0.15f));
            if (angleDelta != 0.0f || angleDelta != angle)
            {
                transform.eulerAngles = new Vector3(current.start_forward.x,
                    current.start_forward.y + angleDelta,
                    current.start_forward.z);
            }

            Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, takeDelta / (current.duration - (current.duration / takeMoveDurationDenom)));
            this.transform.position = position;
        }
        else if (items.Count > 0)
        {
            GameObject item = items[0];
            float scaleDelta = current.duration - takeDelta;
            item.transform.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), scaleDelta / ((current.duration / takeMoveDurationDenom)));
            if ((scaleDelta / ((current.duration / takeMoveDurationDenom))) <= 0.02f)
                if (items.Contains(item))
                {
                    current.object_target.transform.SetParent(GameObject.Find("Env/Objects").transform);
                    items.Remove(item);
                    lastItem = item;
                }
        }

        if (takeDelta >= current.duration)
        {
            takeDelta = 0.0f;
            isTaking = false;
        }
    }

    /*
     * Rather than use it is the action of mouting an object for the moment.
     * Use or mount. By mounting, the main object will be able to "ride" the target.
     * This means that the user will move and interact with the world through the mount.
     */
    public void Use()
    {
        useDelta = Manager.Instance.GetTimeCursor() - current.start;

        // The object should move till its near the target
        // then we should perform the "take" animation.
        // For the moment, we say that the take animation is one thenth of the action duration, the rest being the approch
        if (useDelta < current.duration - (current.duration / 2f))
        {
            if (this.mount == current.object_target)
            {
                this.mount = null;
                current.object_target.transform.SetParent(init_parent.transform);
                current.object_target.GetComponent<PopupObjectMenu>().clickable = true;
                current.object_target.GetComponent<BoxCollider>().enabled = true;
            }

            if (!angleSet)
            {
                Vector3 dir = Vector3.Normalize(current.object_target.transform.position - transform.position);
                float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
                Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);
                angleSet = true;
            }
            float angleDelta = Mathf.Lerp(0, angle, useDelta / (0.15f));
            if (angleDelta != 0.0f || angleDelta != angle)
            {
                transform.eulerAngles = new Vector3(current.start_forward.x,
                    current.start_forward.y + angleDelta,
                    current.start_forward.z);
            }

            Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, useDelta / (current.duration - (current.duration / 2f)));
            this.transform.position = position;
        }
        else
        {
            if (this.mount != current.object_target)
                this.mount = current.object_target;
            current.object_target.transform.SetParent(this.transform);
            current.object_target.GetComponent<PopupObjectMenu>().clickable = false;
            current.object_target.GetComponent<BoxCollider>().enabled = false;
        }

        if (useDelta >= current.duration)
        {
            useDelta = 0.0f;
            isTaking = false;
        }
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