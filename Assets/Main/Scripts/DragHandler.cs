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

	private Bounds CalculateLocalBounds(GameObject ghostObject)
	{
		Quaternion currentRotation = ghostObject.transform.rotation;
		ghostObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

		Bounds bounds = new Bounds(ghostObject.transform.position, Vector3.zero);

		foreach (Renderer renderer in ghostObject.GetComponentsInChildren<Renderer>())
		{
			bounds.Encapsulate(renderer.bounds);
		}

		Vector3 localCenter = bounds.center - ghostObject.transform.position;
		bounds.center = localCenter;

		ghostObject.transform.rotation = currentRotation;
		return bounds;
	}

	private void GhostMode()
    {
		ghostObject = Instantiate(prefab, Camera.main.ScreenToWorldPoint(transform.position), Quaternion.identity);

		Renderer[] rends;
		rends = ghostObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer rend in rends)
		{
			var mats = new Material[rend.materials.Length];
			for (var j = 0; j < rend.materials.Length; j++)
			{
				mats[j] = Manager.Instance.GhostMat;
			}
			rend.materials = mats;
		}

		// The following code is to center and corretly place the ghost object
		GameObject parent = new GameObject("ModelParts");
		List<Transform> children = new List<Transform>();

		parent.transform.SetParent(ghostObject.transform);
		parent.transform.localPosition = new Vector3(0, 0, 0);
		foreach (Transform child in ghostObject.transform)
			children.Add(child);
		foreach (Transform child in children)
			child.SetParent(parent.transform);
		children.Clear();

		ghostObject.transform.GetChild(0).localEulerAngles = new Vector3(0, 90, 0);
		//ghostObject.transform.eulerAngles = new Vector3(0, 90, 0);
		Bounds bounds = CalculateLocalBounds(ghostObject);
		ghostObject.transform.GetChild(0).localPosition = new Vector3(bounds.extents.x, ghostObject.transform.GetChild(0).localPosition.y, ghostObject.transform.GetChild(0).localPosition.z);
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
					ghostObject.transform.eulerAngles = new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * 100 + ghostObject.transform.eulerAngles.y, 0);
				}
			}
		}
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

		Quaternion rot = ghostObject.transform.rotation;
		Vector3 eulers = ghostObject.transform.eulerAngles;
		
		Destroy(ghostObject);

		// Check if the mouse was clicked over a UI element
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			Manager.Instance.SpawnPrefab(prefab, rot, eulers);

			//Manager.Instance.SpawnPrefab(prefab, ghostObject.transform);
		}

		Destroy(ghostObject);
	}

	#endregion
}
