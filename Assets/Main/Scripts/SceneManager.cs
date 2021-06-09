using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public GameObject ScenePanel;

    public GameObject Content;
    public GameObject ContentPrefab;

    public List<GameObject> Scenes;

    private void Awake()
    {
        SaveSystem.Init();
    }

    private void OnDisable()
    {
        if (Manager.Instance != null)
            Manager.Instance.sceneSelected = null;
    }

    private void Start()
    {
        FileInfo[] saveFiles = SaveSystem.GetSaveFiles();

        foreach (FileInfo fileInfo in saveFiles)
        {
            if (!fileInfo.Name.Contains("meta"))
            {
                AddToScrollview(fileInfo);
            }
        }

        Manager.Instance.SwitchShowWindow(ScenePanel);
    }

    private void AddToScrollview(FileInfo fileInfo)
    {
        GameObject scene = Instantiate(ContentPrefab);
        scene.transform.SetParent(Content.transform);

        scene.AddComponent<SceneFileScr>();
        scene.GetComponent<SceneFileScr>().nameTag = fileInfo.Name;
        scene.GetComponent<SceneFileScr>().sceneManager = this.gameObject.GetComponent<SceneManager>();

        AddButtonListener(scene);

        Scenes.Add(scene);
    }

    private void AddButtonListener(GameObject scene)
    {
        scene.GetComponent<SceneFileScr>().btn.onClick.AddListener(delegate { TaskOnClick(scene); });
    }

    private void RemoveButtonListener(GameObject scene)
    {
        scene.GetComponent<SceneFileScr>().btn.onClick.RemoveListener(delegate { TaskOnClick(scene); });
    }

    public void DestroyScene()
    {
        GameObject scene = null;

        if (Manager.Instance != null)
            scene = Manager.Instance.sceneSelected;

        if (scene)
        {
            RemoveButtonListener(scene);

            Scenes.Remove(scene);

            FileInfo[] saveFiles = SaveSystem.GetSaveFiles();

            foreach (FileInfo fileInfo in saveFiles)
            {
                if (scene.GetComponent<SceneFileScr>().nameTag.Contains(fileInfo.Name))
                {
                    fileInfo.Delete();
                }
            }

            Destroy(scene);
            // Destroy scene save File;
        }

        if (Manager.Instance != null)
            Manager.Instance.sceneSelected = null;
    }

    public void TaskOnClick(GameObject scene)
    {
        if (Manager.Instance != null)
            Manager.Instance.sceneSelected = scene;
    }

    public void CreateNewScene(string sceneName)
    {

    }

    public void SaveScene()
    {
        SaveObject saveObject = new SaveObject
        {
            objects = null,
        };

        string json = JsonUtility.ToJson(saveObject);
        string nameTag = SaveSystem.Save(json);

        FileInfo[] saveFiles = SaveSystem.GetSaveFiles();

        foreach (FileInfo fileInfo in saveFiles)
        {
            if (nameTag.Contains(fileInfo.Name))
            {
                AddToScrollview(fileInfo);
            }
        }

    }

    public void LoadScene(string sceneName)
    {
        string saveString = SaveSystem.Load();
        if (saveString != null)
        {
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);

            // Here apply all data to manager
        }
        else
        {
            Debug.Log("No save");
        }
    }

    public void DeleteScene(string sceneName)
    {

    }

    private class SaveObject
    {
        public List<GameObject> objects;
    }
}
