using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public static GameObject itemBeingDragged;
	Vector3 startPosition;
	Transform startParent;
	private CanvasGroup canvasGroup;

	public GameObject prefab;

	private void Awake()
    {
		canvasGroup = GetComponent<CanvasGroup>();
	}

    #region IBeginDragHandler implementation

    public void OnBeginDrag(PointerEventData eventData)
	{
		itemBeingDragged = gameObject;
		startPosition = transform.position;
		startParent = transform.parent;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.alpha = .6f;
	}

	#endregion

	#region IDragHandler implementation

	public void OnDrag(PointerEventData eventData)
	{
		transform.position = eventData.position;
	}

	#endregion

	#region IEndDragHandler implementation

	public void OnEndDrag(PointerEventData eventData)
	{
		itemBeingDragged = null;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.alpha = 1f;
		transform.position = startPosition;

		// Check if the mouse was clicked over a UI element
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			Manager.Instance.SpawnPrefab(prefab);
		}

		//Manager.Instance.SpawnPrefab(prefab);

		//if (transform.parent == startParent)
		//{
		//	transform.position = startPosition;
		//}
		//else
		//{
		//	GameObject tmp = Instantiate(this.gameObject);
		//	tmp.transform.position = startPosition;
		//	tmp.transform.SetParent(startParent);

		//	canvasGroup.blocksRaycasts = false;
		//}
	}

	#endregion
}
