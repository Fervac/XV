using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampPopup : MonoBehaviour
{
    public GameObject popup;

    private void Update()
    {
        Vector3 popupPos = Camera.main.WorldToScreenPoint(transform.position);
        popup.transform.position = popupPos;
    }

    public void CreatePopup()
    {
        popup = Instantiate(Manager.Instance.PopupPrefab, Manager.Instance.GlobalCanvas.transform);
    }
}
