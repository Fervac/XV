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

    public Vector3 init_pos;
    public Vector3 init_rot;
    public Vector3 init_scale;
    public GameObject init_parent;

    private List<Vector3> pos_hist = new List<Vector3>();
    private List<Vector3> rot_hist = new List<Vector3>();

    public string prefabIdentifier = "";

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
        init_scale = this.transform.localScale;
        init_parent = this.transform.parent.gameObject;
    }

    public void ResetModel()
    {
        current = null;
        mount = null;
        items.Clear();

        ResetVariables();

        this.transform.position = init_pos;
        this.transform.eulerAngles = init_rot;
        this.transform.localScale = init_scale;
        this.transform.SetParent(init_parent.transform);
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
            Action old = this.current;
            this.current = action;
            switch(action.type)
            {
                case actionType.MOVE:
                    this.transform.position = action.end_pos;
                    this.transform.eulerAngles = new Vector3(action.start_forward.x,
                        action.start_forward.y + action.angle,
                        action.start_forward.z);
                    if (this.mount)
                        this.mount.transform.position = this.transform.position;
                    break;
                /*case actionType.TAKE:
                    this.transform.position = action.end_pos;
                    action.object_target.transform.position = action.end_pos;
                    action.object_target.transform.localScale = new Vector3(0f, 0f, 0f);
                    break;
                case actionType.PUT:
                    this.transform.position = action.end_pos;
                    action.object_target.transform.position = action.end_pos;
                    if (lastItem)
                        lastItem.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                case actionType.USE:
                    if (mount && action.umount)
                    {
                        action.object_target.transform.SetParent(init_parent.transform);
                        action.object_target.GetComponent<PopupObjectMenu>().clickable = true;
                        action.object_target.GetComponent<BoxCollider>().enabled = true;
                        action.object_target.transform.position = action.end_pos;
                        mount = null;
                    }
                    else if (!mount && !action.umount)
                    {
                        action.object_target.transform.SetParent(this.transform);
                        action.object_target.GetComponent<PopupObjectMenu>().clickable = false;
                        action.object_target.GetComponent<BoxCollider>().enabled = false;
                        action.object_target.transform.position = action.end_pos;
                        mount = action.object_target;
                    }
                    break;*/
                case actionType.TAKE:
                    Take(action.start + action.duration - (action.duration / 2f) - 0.001f);
                    Take(action.start + action.duration);
                    break;
                case actionType.PUT:
                    Put(action.start + action.duration - (action.duration / 2f) - 0.001f);
                    Put(action.start + action.duration);
                    break;
                case actionType.USE:
                    Use(action.start + action.duration - (action.duration / 2f) - 0.001f);
                    Use(action.start + action.duration);
                    break;
                default:
                    break;
            }
            this.current = old;
            return true;
        }
        return false;
    }

    /*
     * Function used to dispatch action behavior. 
     */
    public void PlayAction(Action action)
    {
        if (action != current || Manager.Instance.GetTimeCursor() == 0.0f)
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

    #region Useful Functions

    public void UpdateTransformData(Vector3 npos, Vector3 neuler)
    {
        pos_hist.Add(npos);
        rot_hist.Add(neuler);
    }

    public void DeleteTransformData(int index = -1)
    {
        if (index == -1)
        {
            pos_hist.RemoveAt(pos_hist.Count - 1);
            rot_hist.RemoveAt(rot_hist.Count - 1);
        }
        else if (index >= 0 && (index < pos_hist.Count && index < rot_hist.Count))
        {
            pos_hist.RemoveAt(index);
            rot_hist.RemoveAt(index);
        }
    }

    #endregion

    #region Animation Functions

    public void Move(float forceDelta = -1f)
    {
        if (forceDelta == -1f)
            moveDelta = Manager.Instance.GetTimeCursor() - current.start;
        else
            moveDelta = forceDelta;
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

        if (this.mount)
        {
            this.mount.transform.position = position;
            this.mount.transform.eulerAngles = transform.eulerAngles;
        }

        // Move stuff (and rotate stuff ?)
        if (moveDelta >= current.end)
        {
            moveDelta = 0.0f;
            isMoving = false;
        }
    }

    /*
     * Use to take an object from world to inventory
     * The action is composed of two step :
     *      1 - Go to the target (should not be present in inventory)
     *      2 - Take the target (add into inventory)
     *      
     *  The main difficulty is to know where should be the target at X point. Playing the timeline/action is easy, rewind is harder.
     */
    public void Take(float forceDelta = -1f)
    {
        if (forceDelta == -1f)
            takeDelta = Manager.Instance.GetTimeCursor() - current.start;
        else
            takeDelta = forceDelta;

        if (!angleSet)
        {
            Vector3 dir = Vector3.Normalize(current.end_pos - current.start_pos);
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

        Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, takeDelta / (current.duration - (current.duration / 2f)));
        this.transform.position = position;
        float scaleDelta = current.duration - takeDelta;
        Vector3 scale = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), scaleDelta / ((current.duration / 2f)));
        current.object_target.transform.localScale = scale;
        if (takeDelta < (current.duration - (current.duration / 2f)))
        {
            current.object_target.transform.localScale = new Vector3(1, 1, 1);
            current.object_target.transform.position = current.end_pos;
            current.object_target.transform.SetParent(GameObject.Find("Env/Objects").transform);
            //current.object_target.GetComponent<ModelManager>().UpdateTransformData(current.end_pos, current.object_target.transform.eulerAngles);
            if (items.Contains(current.object_target))
                items.Remove(current.object_target);
        }
        else
        {
            if (!items.Contains(current.object_target))
            {
                items.Add(current.object_target);
                //current.object_target.GetComponent<ModelManager>().UpdateTransformData(current.end_pos, current.object_target.transform.eulerAngles );
                current.object_target.transform.localScale = new Vector3(0, 0, 0);
                current.object_target.transform.position = current.end_pos;
                current.object_target.transform.SetParent(this.transform);
            }
        }
    }

    /*
     * Use to put an object in inventory somewhere in the world
     * The action is composed of two step :
     *      1 - Go to the target point
     *      2 - Put the target at point
     */
    public void Put(float forceDelta = -1f)
    {
        if (forceDelta == -1f)
            takeDelta = Manager.Instance.GetTimeCursor() - current.start;
        else
            takeDelta = forceDelta;

        if (!angleSet)
        {
            Vector3 dir = Vector3.Normalize(current.end_pos - current.start_pos);
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

        Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, takeDelta / (current.duration - (current.duration / 2f)));
        this.transform.position = position;
        float scaleDelta = current.duration - takeDelta;
        Vector3 scale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), scaleDelta / ((current.duration / 2f)));
        current.object_target.transform.localScale = scale;
        if (takeDelta < (current.duration - (current.duration / 2f)))
        {
            current.object_target.transform.localScale = new Vector3(0, 0, 0);
            current.object_target.transform.position = current.start_pos;
            current.object_target.transform.SetParent(this.transform);
            if (!items.Contains(current.object_target))
                items.Add(current.object_target);
        }
        else
        {
            if (items.Contains(current.object_target))
            {
                items.Remove(current.object_target);
                current.object_target.transform.localScale = new Vector3(1, 1, 1);
                current.object_target.transform.position = current.end_pos;
                current.object_target.transform.SetParent(GameObject.Find("Env/Objects").transform);
                lastItem = current.object_target;
            }
        }
    }

    /*
     * Rather than use it is the action of mouting an object for the moment.
     * Use or mount. By mounting, the main object will be able to "ride" the target.
     * This means that the user will move and interact with the world through the mount.
     */
    public void Use(float forceDelta = -1f)
    {
        if (forceDelta == -1f)
            useDelta = Manager.Instance.GetTimeCursor() - current.start;
        else
            useDelta = forceDelta;

        if (!current.umount)
        {
            if (!angleSet)
            {
                Vector3 dir = Vector3.Normalize(current.end_pos - current.start_pos);
                angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
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
        if (useDelta < (current.duration - (current.duration / 2f)))
        {
            /*
             * Here we are in the interval where the model should be moving toward the target.
             * We have to be aware of 2 cases.
             *  - Mounting a object
             *  - Unmounting a object
             */
            if (current.umount)
            {
                current.object_target.transform.SetParent(this.transform, true);
                current.object_target.GetComponent<PopupObjectMenu>().clickable = false;
                current.object_target.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                this.mount = current.object_target;
                this.mount.transform.SetParent(this.mount.GetComponent<ModelManager>().init_parent.transform);
                this.mount.GetComponent<PopupObjectMenu>().clickable = true;
                this.mount.GetComponent<BoxCollider>().enabled = true;
            }
        }
        else
        {
            /*
             * Here we are in the interval after the move. The mounting or unmounting should be done here.
             */
            if (current.umount)
            {
                if (this.mount)
                {
                    this.mount.transform.position = this.transform.position;
                    this.mount.transform.SetParent(this.mount.GetComponent<ModelManager>().init_parent.transform);
                    this.mount.GetComponent<PopupObjectMenu>().clickable = true;
                    this.mount.GetComponent<BoxCollider>().enabled = true;
                    this.mount = null;
                }
            }
            else
            {
                this.mount = current.object_target;
                //this.transform.forward = this.mount.transform.forward;
                this.mount.transform.SetParent(this.transform, true);
                this.mount.transform.position = this.transform.position;
                this.mount.GetComponent<PopupObjectMenu>().clickable = false;
                this.mount.GetComponent<BoxCollider>().enabled = false;
            }
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