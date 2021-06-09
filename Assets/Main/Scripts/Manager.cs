using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    #region SINGLETON PATTERN
    public static Manager _instance;
    public static Manager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Manager>();

                if (_instance == null)
                {
                    GameObject container = new GameObject("Manager");
                    _instance = container.AddComponent<Manager>();
                }

                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }
    }
    #endregion

    public List<GameObject> loadedObjects;

    public GameObject ObjetParent;

    public GameObject PopupPrefab;
    public GameObject GlobalCanvas;

    public Timeline timeline;
    public TimelineEventDetailPanel eventDetailPanel;
    public GameObject eventToolTip;

    public Material GhostMat;
    public FlexibleColorPicker fcp;

    private bool popup = true;

    private void Start()
    {
        loadedObjects = new List<GameObject>();
    }

    #region TimelineManagement

    public bool IsPlaying() { return timeline.IsPlaying(); }
    public float GetTimeCursor() { return timeline.timeCursor; }
    public float GetDuration() { return timeline.duration; }
    public GameObject GetEventTooltip() { return eventToolTip; }

    #endregion

    #region ObjectManagement

    public void DeleteFromLoadedList(GameObject obj)
    {
        if (loadedObjects.Contains(obj))
            loadedObjects.Remove(obj);
    }

    public GameObject AddToLoadedList(GameObject prefab)
    {
        loadedObjects.Add(prefab);

        GameObject tmp = loadedObjects[loadedObjects.Count - 1];

        return tmp;
    }

    public void SpawnPrefab(GameObject prefab, Quaternion _rot, Vector3 _eulers)
    {
        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                GameObject tmp = Instantiate(prefab, hit.point, _rot);
                SetBoxCollider(tmp);

                tmp.AddComponent<PopupObjectMenu>();
                tmp.AddComponent<ModelManager>();
                tmp.AddComponent<DragObject>();

                tmp.transform.eulerAngles = _eulers;
                tmp.transform.SetParent(ObjetParent.transform);
                AddToLoadedList(tmp);
            }
        }
    }

    private Bounds CalculateLocalBounds(GameObject ghostObject)
    {
        Quaternion currentRotation = ghostObject.transform.rotation;
        ghostObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds bounds = new Bounds(ghostObject.transform.position, Vector3.zero);

        foreach (Renderer renderer in ghostObject.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(renderer.bounds);

        Vector3 localCenter = bounds.center - ghostObject.transform.position;
        bounds.center = localCenter;

        ghostObject.transform.rotation = currentRotation;
        return bounds;
    }

    private void SetBoxCollider(GameObject _object)
    {
        _object.AddComponent<BoxCollider>();
        
        Bounds bounds = CalculateLocalBounds(_object);
        BoxCollider box = _object.GetComponent<BoxCollider>();

        box.center = bounds.center;
        box.size = bounds.extents * 2;
        // Disable box collider before click is released
        box.enabled = false;
    }

    public void TogglePopUp(bool force = false, bool state = false)
    {
        if (force)
            popup = state;
        else
            popup = !popup;
        foreach (GameObject obj in loadedObjects)
        {
            PopupObjectMenu _popup = obj.GetComponent<PopupObjectMenu>();
            if (_popup)
                _popup.clickable = popup;
        }
    }

    #endregion

    public void SwitchShowWindow(GameObject window)
    {
        window.SetActive(!window.activeInHierarchy);
    }
}
