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
    private GameObject takeButton;
    private GameObject colorButton;
    private GameObject closeButton;
    private InputField nameField;
    private String nameTag;

    private FlexibleColorPicker fcp;
    private bool _coloring = false;

    public bool clickable = true;

    #region Camera parameters
    private Vector3 _endpoint = new Vector3(0, 0, 0);
    public Vector3 endpoint
    {
        get { return _endpoint; }

        set
        {
            if (_endpoint == value)
                return;

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
            if (_mountTarget == value)
                return;

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
            if (_takeTarget == value)
                return;

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
        takeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "TakeButton").gameObject;
        colorButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "ColorButton").gameObject;
        closeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "CloseButton").gameObject;
        nameField = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "NameField").gameObject.GetComponent<InputField>();

        OnChangeEndPoint += OnChangeEndPointHandler;
        OnChangeMount += OnChangeMountHandler;
        OnChangeTake += OnChangeTakeHandler;
    }

    private void OnChangeEndPointHandler(Vector3 point)
    {
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
        takeButton.GetComponent<Button>().onClick.AddListener(() => TakeObject());
        colorButton.GetComponent<Button>().onClick.AddListener(() => ColorObject());
        closeButton.GetComponent<Button>().onClick.AddListener(() => CloseWindow());

        nameField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        nameTag = this.gameObject.name;
        nameField.text = nameTag;
    }

    private void ValueChangeCheck()
    {
        nameTag = nameField.text;
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && clickable)
        {
            ManageWindow();
            Manager.Instance.timeline.PauseTimeline(); // TODO : Not sure about this
        }
    }

    private void DestroyObject()
    {
        destroyButton.GetComponent<Button>().onClick.RemoveListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.RemoveListener(() => RotateObject());
        closeButton.GetComponent<Button>().onClick.RemoveListener(() => CloseWindow());
        moveButton.GetComponent<Button>().onClick.RemoveListener(() => MoveObject());
        mountButton.GetComponent<Button>().onClick.RemoveListener(() => MountObject());
        takeButton.GetComponent<Button>().onClick.RemoveListener(() => TakeObject());
        colorButton.GetComponent<Button>().onClick.RemoveListener(() => ColorObject());
        OnChangeEndPoint -= OnChangeEndPointHandler;
        OnChangeMount -= OnChangeMountHandler;
        OnChangeTake -= OnChangeTakeHandler;
        nameField.onValueChanged.RemoveListener(delegate { ValueChangeCheck(); });

        Manager.Instance.timeline.DeleteActor(this.gameObject);
        Manager.Instance.DeleteFromLoadedList(this.gameObject);

        ManageWindow();
        Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
        Destroy(this.gameObject);
    }

    private void MoveObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        // Deactivate box collider

        // Display overlay to select new position
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.MOVE;

        this.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    private void MoveObjectAction(Vector3 point)
    {
        if (Camera.main.GetComponent<CameraManager>().overlay_type == overlayType.MOVE && !(point.Equals(new Vector3(0, 0, 0))))
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            // Add action to the timeline
            Action move = new Action(Manager.Instance.timeline.actions.Count, 1f, 0f, 1f, actionType.MOVE, this.gameObject, null,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(move, this.gameObject);
        }

        // Reactivate box collider
        this.gameObject.GetComponent<BoxCollider>().enabled = true;
        Manager.Instance.TogglePopUp(true, true);

        CloseWindow();
    }

    private void MountObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.MOUNT;

        this.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    private void MountObjectAction(Vector3 point, GameObject _mount)
    {
        if (_mount != null)
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            Action mount = new Action(Manager.Instance.timeline.actions.Count, 1f, 0f, 1f, actionType.USE, this.gameObject, _mount,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(mount, this.gameObject);
        }
        this.gameObject.GetComponent<BoxCollider>().enabled = true;
        Manager.Instance.TogglePopUp(true, true);
        CloseWindow();
    }

    private void TakeObject()
    {
        Manager.Instance.TogglePopUp(true, false);
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;
        Camera.main.GetComponent<CameraManager>().overlay_type = overlayType.TAKE;

        this.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    private void TakeObjectAction(Vector3 point, GameObject _take)
    {
        if (_take != null)
        {
            Vector3 dir = Vector3.Normalize(_endpoint - transform.position);
            float angle = Vector3.SignedAngle(transform.forward, dir, new Vector3(0, 1, 0));
            Vector3 endEuler = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y + angle, this.transform.eulerAngles.z);

            Action take = new Action(Manager.Instance.timeline.actions.Count, 1f, 0f, 1f, actionType.TAKE, this.gameObject, _take,
                this.transform.position, _endpoint,
                this.transform.eulerAngles, endEuler);
            Manager.Instance.timeline.AddAction(take, this.gameObject);
        }
        this.gameObject.GetComponent<BoxCollider>().enabled = true;
        Manager.Instance.TogglePopUp(true, true);
        CloseWindow();
    }

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
    }
}
