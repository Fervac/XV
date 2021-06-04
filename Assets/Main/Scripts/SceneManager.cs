using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private void Awake()
    {
        SaveSystem.Init();
    }

    public void CreateNewScene(string sceneName)
    {

    }

    public void SaveScene(string sceneName)
    {
        SaveObject saveObject = new SaveObject
        {
            objects = null,
        };

        string json = JsonUtility.ToJson(saveObject);
        SaveSystem.Save(json);
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
