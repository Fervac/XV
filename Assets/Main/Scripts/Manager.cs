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

    public GameObject AssetsListPanel;

    public GameObject ObjetParent;

    public GameObject PopupPrefab;
    public GameObject GlobalCanvas;

    public Timeline timeline;
    public TimelineEventDetailPanel eventDetailPanel;
    public GameObject eventToolTip;

    public Material GhostMat;
    public FlexibleColorPicker fcp;

    private bool popup = true;
    public GameObject sceneSelected;

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

    public GameObject AddToLoadedList(GameObject _object)
    {
        loadedObjects.Add(_object);

        GameObject tmp = loadedObjects[loadedObjects.Count - 1];

        return tmp;
    }

    private void UpdateName(GameObject _object)
    {
        int id = 0;
        GameObject duplicate = null;
        string updatedName = _object.name;

        duplicate = GameObject.Find(updatedName);
        if (duplicate == _object)
            return;
        while (duplicate)
        {
            if (duplicate && duplicate != _object)
            {
                ++id;
                print("error, " + updatedName + " already exist. Renaming to " + _object.name + "-" + id.ToString());
                updatedName = _object.name + "-" + id.ToString();
            }
            else if (duplicate == _object)
                break;
            duplicate = GameObject.Find(updatedName);
        }
        if (updatedName != _object.name)
            _object.name = updatedName;
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
                UpdateName(tmp);
                SetBoxCollider(tmp);

                tmp.AddComponent<PopupObjectMenu>();
                tmp.AddComponent<ModelManager>();
                tmp.GetComponent<ModelManager>().prefabIdentifier = prefab.name;
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

    public List<GameObject> GetAssetsList()
    {
        List<GameObject> assets = new List<GameObject>();

        foreach (Transform child in AssetsListPanel.transform)
        {
            DragHandler dh = child.gameObject.GetComponent<DragHandler>();
            if (!dh)
            {
                Debug.LogError("Error no DragHandler component on Asset item");
                continue;
            }

            if (dh.prefab != null)
                assets.Add(dh.prefab);
        }

        return assets;
    }

    #region SaveLoadManagement

    public void ResetScene()
    {
        List<GameObject> SceneObjects = new List<GameObject>(loadedObjects);

        foreach (GameObject obj in SceneObjects)
        {
            DeleteFromLoadedList(obj);
            if (GameObject.Find(obj.name))
                Destroy(obj);
        }
    }

    public Dictionary<int, GameObject> LoadObjects(List<GameObjectSaveData> savedObjects)
    {
        // Get a list of the assets
        List<GameObject> prefabList = GetAssetsList();
        GameObject model = null, spawned = null;
        ModelManager modelManager = null;

        Dictionary<int, GameObject> instances = new Dictionary<int, GameObject>();

        // Destroy every object in current scene
        ResetScene();

        // Loop through each GameObject saved data to recreate them
        foreach (GameObjectSaveData obj in savedObjects)
        {
            // Find the corresponding prefab
            model = prefabList.Find(x => x.name == obj.prefabName);

            // Not found ? Then abort this pelicular object
            if (!model)
            {
                Debug.LogWarning("Model '" + obj.prefabName + "' not found !");
                continue;
            }

            // Spawn object from prefab
            SpawnPrefab(model, Quaternion.identity, new Vector3(0, 0, 0));
            spawned = loadedObjects[loadedObjects.Count - 1];
            instances.Add(obj.instanceId, spawned);

            // Use data from the GameObjectSaveData struct to correctly setup the object
            spawned.name = obj.name;
            spawned.transform.position = obj.init_pos;
            spawned.transform.eulerAngles = obj.init_rot;
            spawned.transform.localScale = obj.init_scale;
            spawned.transform.SetParent(obj.init_parent.transform);

            modelManager = spawned.GetComponent<ModelManager>();
            modelManager.init_pos = obj.init_pos;
            modelManager.init_rot = obj.init_rot;
            modelManager.init_scale = obj.init_scale;
            modelManager.init_parent = obj.init_parent;

            spawned.GetComponent<PopupObjectMenu>().PubNameTag = obj.nameTag;
        }

        return instances;
    }

    public void LoadTimeline(List<Action> actions, Dictionary<int, GameObject> instances, float duration)
    {
        // Clear the timeline before loading new actions
        timeline.ClearTimeline();

        timeline.SetDuration(duration);

        Action act = null;
        GameObject _op = null, _target = null;

        foreach (Action action in actions)
        {
            act = new Action(action);

            if (action.op_iid == 0 || !instances.TryGetValue(action.op_iid, out _op))
            {
                Debug.LogError("Error ! Bad operator instanceId : " + action.op_iid);
                continue;
            }

            act.object_operator = _op;
            act.op_iid = _op.GetInstanceID();

            if (action.tar_iid != 0 && instances.TryGetValue(action.tar_iid, out _target))
            {
                act.object_target = _target;
                act.tar_iid = _target.GetInstanceID();
            }

            timeline.AddAction(act, _op);
        }
    }

    #endregion
}
