using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public List<GameObject> importedObjects;

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

    public GameObject SceneInput;
    public string currentSceneName = "";

    public List<GameObject> characters;
    public int charIndex = 0;

    public CameraManager camKaren;

    string objPath = string.Empty;
    public GameObject import;

    [System.Obsolete]
    private void Start()
    {
        loadedObjects = new List<GameObject>();
        importedObjects = new List<GameObject>();
        characters = new List<GameObject>();

        if (!Directory.Exists(Application.persistentDataPath + "/UserImports/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/UserImports/");
        }

        var info = new DirectoryInfo(Application.persistentDataPath + "/UserImports/");
        var fileInfo = info.GetFiles();

        SwitchShowWindow(import);

        foreach (FileInfo file in fileInfo)
        {
            objPath = file.FullName;

            import.GetComponent<ObjFromFileTest>().objPath = objPath;

            if (file.Extension == ".obj")
                import.GetComponent<ObjFromFileTest>().LoadObject();
        }

        SwitchShowWindow(import);
    }

    #region CharacterViewManagement

    public GameObject GetCurrentFirstPerson()
    {
        GameObject subCam = null;
        if (characters.Count < 1)
            return subCam;
        if (charIndex < characters.Count)
            return characters[charIndex];
        else
            return null;
    }

    public GameObject GetNextFirstPerson()
    {
        GameObject subCam = null;
        if (characters.Count < 1)
            return subCam;
        else if (characters.Count == 1)
        {
            charIndex = 0;
            subCam = characters[charIndex];
        }
        else
        {
            charIndex++;
            if (charIndex >= characters.Count)
                charIndex = 0;
            subCam = characters[charIndex];
        }

        return subCam;
    }

    public void DeleteCharacter(GameObject character)
    {
        int index = characters.FindIndex(x => x == character);
        if (index == -1)
            return;
        if (charIndex <= index)
            charIndex--;
        if (charIndex < 0)
            charIndex = 0;
        characters.Remove(character);
    }

    #endregion

    #region TimelineManagement

    public bool IsPlaying() { return timeline.IsPlaying(); }
    public float GetTimeCursor() { return timeline.timeCursor; }
    public float GetDuration() { return timeline.duration; }
    public GameObject GetEventTooltip() { return eventToolTip; }

    #endregion

    #region ObjectManagement

    public void DeleteFromImportedList(GameObject obj)
    {
        if (loadedObjects.Contains(obj))
            loadedObjects.Remove(obj);
    }

    public GameObject AddToImportedList(GameObject _object)
    {
        importedObjects.Add(_object);

        GameObject tmp = importedObjects[importedObjects.Count - 1];

        return tmp;
    }
    public void DeleteFromLoadedList(GameObject obj)
    {
        if (characters.Contains(obj))
            DeleteCharacter(obj);
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

    public void SpawnPrefab(GameObject prefab, Quaternion _rot, Vector3 _eulers, bool posModifier = true, bool addCollider = true, bool _override = false)
    {
        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f) || _override)
        {
            if (hit.collider.CompareTag("Floor") || _override)
            {
                GameObject tmp = Instantiate(prefab, hit.point, _rot);
                UpdateName(tmp);
                if (addCollider)
                    SetBoxCollider(tmp);
                else
                    characters.Add(tmp);

                tmp.AddComponent<PopupObjectMenu>();
                tmp.AddComponent<ModelManager>();
                tmp.GetComponent<ModelManager>().posModifier = posModifier;
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
        //box.enabled = false;
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

    public void QuitApp()
    {
        Application.Quit();
    }

    public void OpenVideoFolder()
    {
        Application.OpenURL(RockVR.Video.PathConfig.SaveFolder);
    }
    public void OpenImportFolder()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/UserImports/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/UserImports/");
        }
        Application.OpenURL(Application.persistentDataPath + "/UserImports/");
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

    public void UpdateSceneName(string sceneName)
    {
        this.currentSceneName = sceneName.Trim();
        this.currentSceneName = Regex.Replace(this.currentSceneName, @"[^a-zA-Z0-9 _]", "");
    }

    #region SaveLoadManagement

    public void ResetScene()
    {
        List<GameObject> SceneObjects = new List<GameObject>(loadedObjects);

        foreach (GameObject obj in SceneObjects)
        {
            DeleteFromLoadedList(obj);
            obj.GetComponent<PopupObjectMenu>().DestroyFromAfar();
            if (GameObject.Find(obj.name))
                Destroy(obj);
        }

        loadedObjects.Clear();
        loadedObjects = new List<GameObject>();
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
            SpawnPrefab(model, Quaternion.identity, new Vector3(0, 0, 0), !(obj.prefabName == "UsableCharacter"), !(obj.prefabName == "UsableCharacter"), true);
            spawned = loadedObjects[loadedObjects.Count - 1];
            instances.Add(obj.instanceId, spawned);

            // Use data from the GameObjectSaveData struct to correctly setup the object
            spawned.name = obj.name;
            spawned.transform.position = obj.init_pos;
            spawned.transform.eulerAngles = obj.init_rot;
            spawned.transform.localScale = obj.init_scale;
            //spawned.transform.SetParent(obj.init_parent.transform);

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

        if (instances == null || instances.Count <= 0)
            return;

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
