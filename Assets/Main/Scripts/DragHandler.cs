using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static GameObject itemBeingDragged;
	Vector3 startPosition;
	private CanvasGroup canvasGroup;

	public GameObject prefab;
	private GameObject ghostObject;

	private void Awake()
    {
		canvasGroup = GetComponent<CanvasGroup>();
	}

    #region IBeginDragHandler implementation

    public void OnBeginDrag(PointerEventData eventData)
	{
		itemBeingDragged = gameObject;
		startPosition = transform.position;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.alpha = .6f;

		GhostMode();
	}

    private void GhostMode()
    {
		ghostObject = Instantiate(prefab, Camera.main.ScreenToWorldPoint(transform.position), Quaternion.identity);
		//ghostObject.GetComponent<MeshRenderer>().material = Manager.Instance.GhostMat;
	}

    private void Update()
    {
        if (ghostObject)
        {
			Ray ray;
			RaycastHit hit;
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100.0f))
			{
				if (hit.collider.CompareTag("Floor"))
				{
					ghostObject.transform.position = hit.point;
				}
			}
		}
    }

    #endregion

    #region IDragHandler implementation

    public void OnDrag(PointerEventData eventData)
	{
		transform.position = eventData.position;
		//ghostObject.transform.position = Camera.main.ScreenToWorldPoint(eventData.position);
	}

	#endregion

	#region IEndDragHandler implementation

	public void OnEndDrag(PointerEventData eventData)
	{
		itemBeingDragged = null;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.alpha = 1f;
		transform.position = startPosition;

		Destroy(ghostObject);

		// Check if the mouse was clicked over a UI element
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			Manager.Instance.SpawnPrefab(prefab);
		}
	}

	#endregion
}
