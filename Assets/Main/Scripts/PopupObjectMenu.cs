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
    private GameObject closeButton;

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
        closeButton = Extensions.Search(EmptyObj.GetComponent<ClampPopup>().popup.transform, "CloseButton").gameObject;
    }


    private void Start()
    {
        destroyButton.GetComponent<Button>().onClick.AddListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.AddListener(() => RotateObject());
        closeButton.GetComponent<Button>().onClick.AddListener(() => CloseWindow());
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Manager.Instance.SwitchShowWindow(GetComponentInChildren<ClampPopup>().popup);
        }
    }

    private void DestroyObject()
    {
        destroyButton.GetComponent<Button>().onClick.RemoveListener(() => DestroyObject());
        rotateButton.GetComponent<Button>().onClick.RemoveListener(() => RotateObject());
        closeButton.GetComponent<Button>().onClick.RemoveListener(() => CloseWindow());

        Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
        Destroy(this.gameObject);
    }

    private void RotateObject()
    {
        gameObject.transform.Rotate(0, 90, 0);
    }

    private void CloseWindow()
    {
        Manager.Instance.SwitchShowWindow(EmptyObj.GetComponent<ClampPopup>().popup);
    }
}
