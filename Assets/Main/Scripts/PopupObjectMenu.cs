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
    private GameObject colorButton;
    private GameObject closeButton;

    private bool _coloring = false;

    private Vector3 _endpoint = new Vector3(0, 0, 0);
    public Vector3 endpoint
    {
        get { return _endpoint; }

        set
        {
            if (_endpoint == value)
                return;

            _endpoint = value;

            if (OnChangeEndPoint != null)
                OnChangeEndPoint(_endpoint);
        }
    }

    public delegate void OnChangeEndPointDelegate(Vector3 newVal);
    public event OnChangeEndPointDelegate OnChangeEndPoint;

    private void Awake()
    {
        EmptyObj = new GameObject("placeholder");
        EmptyObj.transform.position = new Vector3(transform.position.x, transform.position.y +1f, transform.position.z);
        EmptyObj.transform.parent = this.gameObject.transform;

        EmptyObj.AddComponent<ClampPopup>();
        EmptyObj.GetComponent<ClampPopup>().CreatePopup();


        //destroyButton = EmptyObj.GetComponent<ClampPopup>().popup.GetComponentInChildren<Button>();

        destroyButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "DestroyButton").gameObject;
        rotateButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "RotateButton").gameObject;
        moveButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "MoveButton").gameObject;
        colorButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "ColorButton").gameObject;
        closeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "CloseButton").gameObject;

        OnChangeEndPoint += OnChangeEndPointHandler;
    }

    private void OnChangeEndPointHandler(Vector3 point)
    {
        MoveObjectAction(point);
    }


    private void Start()
    {
        destroyButton.GetComponent<Button>().onClick.AddListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.AddListener(() => RotateObject());
        moveButton.GetComponent<Button>().onClick.AddListener(() => MoveObject());
        colorButton.GetComponent<Button>().onClick.AddListener(() => ColorObject());
        closeButton.GetComponent<Button>().onClick.AddListener(() => CloseWindow());
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
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
        colorButton.GetComponent<Button>().onClick.RemoveListener(() => ColorObject());
        OnChangeEndPoint -= OnChangeEndPointHandler;

        Manager.Instance.timeline.DeleteActor(this.gameObject);

        ManageWindow();
        Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
        Destroy(this.gameObject);
    }

    private void MoveObject()
    {
        // Deactivate box collider

        // Display overlay to select new position
        Camera.main.GetComponent<CameraManager>()._operator = this.gameObject;
        Camera.main.GetComponent<CameraManager>().overlay = true;

        this.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    private void MoveObjectAction(Vector3 point)
    {
        if (!(point.Equals(new Vector3(0, 0, 0))))
        {
            // Add action to the timeline
            Action move = new Action(Manager.Instance.timeline.actions.Count, 1f, 0f, 1f, actionType.MOVE, this.gameObject, null,
                this.transform.position, _endpoint,
                this.transform.rotation, Quaternion.identity);
            Manager.Instance.timeline.AddAction(move, this.gameObject);
        }

        // Reactivate box collider
        this.gameObject.GetComponent<BoxCollider>().enabled = true;

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
        Manager.Instance.SwitchShowWindow(Manager.Instance.fcp.transform.parent.gameObject);

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
                    mat.SetColor("_Color", Manager.Instance.fcp.color);
                }
            }
        }
    }
}
