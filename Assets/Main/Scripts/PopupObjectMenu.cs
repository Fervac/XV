using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupObjectMenu : MonoBehaviour
{
    private GameObject EmptyObj;
    private GameObject destroyButton;
    private GameObject rotateButton;
    private GameObject moveButton;
    private GameObject mountButton;
    private GameObject unmountButton;
    private GameObject takeButton;
    private GameObject putButton;
    private GameObject colorButton;
    private GameObject closeButton;
    private InputField nameField;
    private string nameTag;

    public string PubNameTag
    {
        get { return nameTag; }
        set
        {
            nameTag = value;
            nameField.text = value;
        }
    }

    private FlexibleColorPicker fcp;
    private bool _coloring = false;

    public bool clickable = true;

    private ModelManager manager = null;
    private bool takeState = true;

    private bool mountState = true;

    #region Camera parameters
    private Vector3 _endpoint = new Vector3(0, 0, 0);
    public Vector3 endpoint
    {
        get { return _endpoint; }

        set
        {
            /*if (_endpoint == value)
                return;*/

            value.y = this.transform.position.y; // To avoid the object going up or down. But this imply that the floor is always at the same level
            _endpoint = value;

            if (OnChangeEndPoint != null)
                OnChangeEndPoint(_endpoint);
        }
    }

    public delegate void OnChangeEndPointDelegate(Vector3 newVal);
    public event OnChangeEndPointDelegate OnChangeEndPoint;

    private GameObject _mountTarget = null;
    public GameObject mountTarget
    {
        get { return _mountTarget; }

        set
        {
            /*if (_mountTarget == value)
                return;*/

            _mountTarget = value;

            if (OnChangeMount != null)
                OnChangeMount(endpoint, _mountTarget);
        }
    }

    public delegate void OnChangeMountDelegate(Vector3 newVal, GameObject obj);
    public event OnChangeMountDelegate OnChangeMount;

    private GameObject _takeTarget = null;
    public GameObject takeTarget
    {
        get { return _takeTarget; }

        set
        {
            /*if (_takeTarget == value)
                return;*/

            _takeTarget = value;

            if (OnChangeTake != null)
                OnChangeTake(endpoint, _takeTarget);
        }
    }

    public delegate void OnChangeTakeDelegate(Vector3 newVal, GameObject obj);
    public event OnChangeTakeDelegate OnChangeTake;

    #endregion

    private void Awake()
    {
        EmptyObj = new GameObject("placeholder");
        EmptyObj.transform.position = new Vector3(transform.position.x, transform.position.y +1f, transform.position.z);
        EmptyObj.transform.parent = this.gameObject.transform;

        EmptyObj.AddComponent<ClampPopup>();
        EmptyObj.GetComponent<ClampPopup>().CreatePopup();

        destroyButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "DestroyButton").gameObject;
        rotateButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "RotateButton").gameObject;
        moveButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "MoveButton").gameObject;
        mountButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "MountButton").gameObject;
        unmountButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "UnmountButton").gameObject;
        takeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "TakeButton").gameObject;
        putButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "PutButton").gameObject;
        colorButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "ColorButton").gameObject;
        closeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "CloseButton").gameObject;
        nameField = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "NameField").gameObject.GetComponent<InputField>();


        OnChangeEndPoint += OnChangeEndPointHandler;
        OnChangeMount += OnChangeMountHandler;
        OnChangeTake += OnChangeTakeHandler;
    }

    private void OnChangeEndPointHandler(Vector3 point)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type == overlayType.PUT)
            PutObjectAction(point, null);
        else
            MoveObjectAction(point);
    }

    private void OnChangeMountHandler(Vector3 point, GameObject _mount)
    {
        MountObjectAction(point, _mount);
    }

    private void OnChangeTakeHandler(Vector3 point, GameObject _take)
    {
        TakeObjectAction(point, _take);
    }

    private void Start()
    {
        destroyButton.GetComponent<Button>().onClick.AddListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.AddListener(() => RotateObject());
        moveButton.GetComponent<Button>().onClick.AddListener(() => MoveObject());
        mountButton.GetComponent<Button>().onClick.AddListener(() => MountObject());
        unmountButton.GetComponent<Button>().onClick.AddListener(() => MountObject());
        takeButton.GetComponent<Button>().onClick.AddListener(() => TakeObject());
        putButton.GetComponent<Button>().onClick.AddListener(() => PutObject());
        colorButton.GetComponent<Button>().onClick.AddListener(() => ColorObject());
        closeButton.GetComponent<Button>().onClick.AddListener(() => CloseWindow());

        if (string.IsNullOrEmpty(nameTag))
            nameTag = this.gameObject.name;
        nameField.text = nameTag;
        nameField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        manager = this.GetComponent<ModelManager>();
        takeState = true;
        putButton.SetActive(false);
    }

    private void ValueChangeCheck()
    {
        if (string.IsNullOrEmpty(nameField.text))
            return;
        nameTag = nameField.text;
        // We should update the timeline line (when modifying the gameobject name)
        //Manager.Instance.UpdateActor(manager.);
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && clickable)
        {
            ManageWindow();
            Manager.Instance.timeline.PauseTimeline(); // TODO : Not sure about this
        }
    }

    public void DestroyFromAfar() { DestroyObject(true); } // Used to reset scene

    private void DestroyObject(bool onlyPop = false)
    {
        destroyButton.GetComponent<Button>().onClick.RemoveListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.RemoveListener(() => RotateObject());
        closeButton.GetComponent<Button>().onClick.RemoveListener(() => CloseWindow());
        moveButton.GetComponent<Button>().onClick.RemoveListener(() => MoveObject());
        mountButton.GetComponent<Button>().onClick.RemoveListener(() => MountObject());
        unmountButton.GetComponent<Button>().onClick.RemoveListener(() => MountObject());
        takeButton.GetComponent<Button>().onClick.RemoveListener(() => TakeObject());
        putButton.GetComponent<Button>().onClick.RemoveListener(() => PutObject());
        colorButton.GetComponent<Button>().onClick.RemoveListener(() => ColorObject());
        OnChangeEndPoint -= OnChangeEndPointHandler;
        OnChangeMount -= OnChangeMountHandler;
        OnChangeTake -= OnChangeTakeHandler;
        nameField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });

        Manager.Instance.timeline.DeleteActor(this.gameObject);
        Manager.Instance.DeleteFromLoadedList(this.gameObject);

        if (!onlyPop)
        {
            ManageWindow();
            Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
            Destroy(this.gameObject);
        }
        else
            Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
    }

    #region Actions handlers

    private float GetActionStartFromCursor()
    {
        float start = Manager.Instance.GetTimeCursor();
        if (start + 1f > Manager.Instance.GetDuration())
            start = 0.0f;

        return (start);
    }

    private void DisableCollider()
    {
        if (this.gameObject.GetComponent<BoxCollider>())
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
        if (this.gameObject.GetComponent<CharacterController>())
            this.gameObject.GetComponent<CharacterController>().enabled = false;
    }

    private void EnableCollider()
    {
        if (this.gameObject.GetComponent<BoxCollider>())
            this.gameObject.GetComponent<BoxCollider>().enabled = true;
        if (this.gameObject.GetComponent<CharacterController>())
            this.gameObject.GetComponent<CharacterController>().enabled = true;
    }

    #region Move action

    private void MoveObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        // Deactivate box collider

        // Display overlay to select new position
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.MOVE;

        DisableCollider();
        CloseWindow();
    }

    private void MoveObjectAction(Vector3 point)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type == overlayType.MOVE && !(point.Equals(new Vector3(0, 0, 0))))
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            float start = GetActionStartFromCursor();

            // Add action to the timeline
            Action move = new Action(Manager.Instance.timeline.actions.Count, 1f, start, start + 1f, actionType.MOVE, this.gameObject, null,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(move, this.gameObject);
        }

        // Reactivate box collider
        EnableCollider();
        Manager.Instance.TogglePopUp(true, true);

        CloseWindow();
    }

    #endregion
    #region Mount action

    private void MountObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        if (mountState)
        {
            Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
            Camera.main.GetComponent<CameraManager>().overlay = true;
            Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.MOUNT;

            DisableCollider();
        }
        else
            UnMountObjectAction();
        CloseWindow();
    }

    private void MountObjectAction(Vector3 point, GameObject _mount)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type != overlayType.MOUNT)
        {
            EnableCollider();
            Manager.Instance.TogglePopUp(true, true);
            CloseWindow();
            return;
        }
        if (_mount != null)
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            float start = GetActionStartFromCursor();

            Action mount = new Action(Manager.Instance.timeline.actions.Count, 1f, start, start + 1f, actionType.USE, this.gameObject, _mount,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(mount, this.gameObject);
            manager.mountEver = _mount;
        }
        EnableCollider();
        Manager.Instance.TogglePopUp(true, true);
        CloseWindow();
    }

    private void UnMountObjectAction()
    {
        float start = GetActionStartFromCursor();

        Action unmount = new Action(Manager.Instance.timeline.actions.Count, 0.5f, start, start + 0.5f, actionType.USE, this.gameObject, manager.mountEver,
            this.transform.position, this.transform.position, // Need to correct the position
            this.transform.eulerAngles, this.transform.eulerAngles);
        unmount.umount = true;
        Manager.Instance.timeline.AddAction(unmount, this.gameObject);
        manager.mountEver = null;
        _mountTarget = null;
        EnableCollider();
        Manager.Instance.TogglePopUp(true, true);
    }

    #endregion
    #region Take action

    private void TakeObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.TAKE;

        DisableCollider();
        CloseWindow();
    }

    private void TakeObjectAction(Vector3 point, GameObject _take)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type != overlayType.TAKE)
        {
            EnableCollider();
            Manager.Instance.TogglePopUp(true, true);
            CloseWindow();
            return;
        }
        if (_take != null)
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            float start = GetActionStartFromCursor();

            Action take = new Action(Manager.Instance.timeline.actions.Count, 1f, start, start + 1f, actionType.TAKE, this.gameObject, _take,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(take, this.gameObject);
            manager.itemsEver.Add(_take);
        }
        EnableCollider();
        Manager.Instance.TogglePopUp(true, true);
        CloseWindow();
    }

    #endregion
    #region Put action

    private void PutObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.PUT;

        DisableCollider();
        CloseWindow();
    }

    private void PutObjectAction(Vector3 point, GameObject _take)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type != overlayType.PUT)
        {
            EnableCollider();
            Manager.Instance.TogglePopUp(true, true);
            CloseWindow();
            return;
        }
        Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
        float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
        Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

        float start = GetActionStartFromCursor();

        Action put = new Action(Manager.Instance.timeline.actions.Count, 1f, start, start + 1f, actionType.PUT, this.gameObject, manager.itemsEver[0],
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
        Manager.Instance.timeline.AddAction(put, this.gameObject);
        manager.itemsEver.Remove(manager.itemsEver[0]);
        EnableCollider();
        Manager.Instance.TogglePopUp(true, true);
        CloseWindow();
    }

    #endregion
    #endregion

    private void RotateObject()
    {
        gameObject.transform.Rotate(0, 90, 0);
    }

    private void CloseWindow()
    {
        ManageWindow();
    }

    private void ColorObject()
    {
        Manager.Instance.SwitchShowWindow(EmptyObj.GetComponent<ClampPopup>().holderFcp);

        _coloring = !_coloring;
    }

    private void ManageWindow()
    {
        if (EmptyObj.GetComponent<ClampPopup>().popup.activeSelf)
        {
            if (_coloring)
            {
                ColorObject();
            }
        }
        Manager.Instance.SwitchShowWindow(EmptyObj.GetComponent<ClampPopup>().popup);
    }

    #region Manage button display

    private void ManageTakeButton()
    {
        if (manager && manager.itemsEver.Count > 0 && takeState)
        {
            takeState = false;
            putButton.SetActive(!takeState);
            takeButton.SetActive(takeState);
        }
        else if (manager && manager.itemsEver.Count == 0 && !takeState)
        {
            takeState = true;
            putButton.SetActive(!takeState);
            takeButton.SetActive(takeState);
        }
    }

    private void ManageMountButton()
    {
        if (manager && manager.mountEver != null && mountState)
        {
            mountState = false;
            unmountButton.SetActive(!mountState);
            mountButton.SetActive(mountState);
        }
        else if (manager && manager.mountEver == null && !mountState)
        {
            mountState = true;
            unmountButton.SetActive(!mountState);
            mountButton.SetActive(mountState);
        }
    }

    #endregion

    private void Update()
    {
        if (_coloring)
        {
            MeshRenderer[] meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer thisMeshRenderer in meshRenderers)
            {
                foreach (Material mat in thisMeshRenderer.materials)
                {
                    mat.SetColor("_Color", EmptyObj.GetComponent<ClampPopup>().fcp.color);
                }
            }
        }

        ManageTakeButton();
        ManageMountButton();
    }
}
