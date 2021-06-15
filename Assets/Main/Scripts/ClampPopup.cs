using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampPopup : MonoBehaviour
{
    public GameObject popup;
    public GameObject holderFcp;
    public FlexibleColorPicker fcp;

    private void Update()
    {
        if (Manager.Instance.camKaren.camMode == cameraMode.Overview)
        {
            Vector3 popupPos = Camera.main.WorldToScreenPoint(transform.position);
            popup.transform.position = popupPos;
        }
    }

    public void CreatePopup()
    {
        popup = Instantiate(Manager.Instance.PopupPrefab, Manager.Instance.GlobalCanvas.transform);
        CreateColorPicker();
    }

    public void CreateColorPicker()
    {
        holderFcp = new GameObject();
        holderFcp.transform.SetParent(popup.transform);
        fcp = Instantiate(Manager.Instance.fcp, Manager.Instance.GlobalCanvas.transform);
        fcp.transform.position = new Vector3(popup.transform.position.x + 200f, popup.transform.position.y, popup.transform.position.z);
        fcp.transform.SetParent(holderFcp.transform);

        holderFcp.SetActive(false);
    }
}
