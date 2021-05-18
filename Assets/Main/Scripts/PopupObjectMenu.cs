using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupObjectMenu : MonoBehaviour
{
    private GameObject EmptyObj;
    private Button destroyButton;

    private void Awake()
    {
        EmptyObj = new GameObject("placeholder");
        EmptyObj.transform.position = new Vector3(transform.position.x, transform.position.y +1f, transform.position.z);
        EmptyObj.transform.parent = this.gameObject.transform;

        EmptyObj.AddComponent<ClampPopup>();
        EmptyObj.GetComponent<ClampPopup>().CreatePopup();

        destroyButton = EmptyObj.GetComponent<ClampPopup>().popup.GetComponentInChildren<Button>();
    }

    private void Start()
    {
        destroyButton.onClick.AddListener(() => actionToObject());
    }

    private void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Manager.Instance.SwitchShowWindow(GetComponentInChildren<ClampPopup>().popup);
        }
    }

    private void OnDestroy()
    {
        destroyButton.onClick.RemoveListener(() => actionToObject());
    }

    void actionToObject()
    {
        Destroy(EmptyObj.GetComponent<ClampPopup>().popup);
        Destroy(this.gameObject);
    }
}
