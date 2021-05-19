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

    List<GameObject> loadedObjects;

    public GameObject PopupPrefab;
    public GameObject GlobalCanvas;

    public Timeline timeline;
    public GameObject eventToolTip;

    public Material GhostMat;

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

    public GameObject AddToLoadedList(GameObject prefab)
    {
        loadedObjects.Add(prefab);

        GameObject tmp = loadedObjects[loadedObjects.Count - 1];

        return tmp;
    }

    public void SpawnPrefab(GameObject prefab)
    {
        Ray ray;
        RaycastHit hit;
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                GameObject tmp = Instantiate(prefab, hit.point, Quaternion.identity);
                tmp.AddComponent<PopupObjectMenu>();
                tmp.AddComponent<ModelManager>();
                tmp.AddComponent<BoxCollider>();
            }
        }
    }

    public void SwitchShowWindow(GameObject window)
    {
        window.SetActive(!window.activeInHierarchy);
    }
}
