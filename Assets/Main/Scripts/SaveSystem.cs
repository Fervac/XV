using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class GameObjectSaveData
{
    public string name;         // GameObject name
    public string nameTag;      // GameObject nameTag (for popup and timeline)
    public string prefabName;   // Model name
    public int instanceId;      // InstanceId used to know which gameobject is used in actions

    public Vector3 init_pos;        // Initial position of the object (from ModelManager)
    public Vector3 init_rot;        // Initial rotation (eulerAngles)
    public Vector3 init_scale;      // Initial scale
    public GameObject init_parent;  // Initial parent (should always be the GameObject "Env/Objects")

    public GameObjectSaveData(GameObject obj)
    {
        name = obj.name;
        prefabName = obj.GetComponent<ModelManager>().prefabIdentifier;
        instanceId = obj.GetInstanceID();

        ModelManager obj_manager = obj.GetComponent<ModelManager>();
        PopupObjectMenu obj_pom = obj.GetComponent<PopupObjectMenu>();

        nameTag = obj_pom.PubNameTag;

        init_pos = obj_manager.init_pos;
        init_rot = obj_manager.init_rot;
        init_scale = obj_manager.init_scale;
        init_parent = obj_manager.init_parent;
    }
}

public static class SaveSystem
{
    private static readonly string SAVE_FOLDER_EDITOR = Application.dataPath + "/Saves/";
    private static readonly string STANDALONE_SAVE_FOLDER = Application.persistentDataPath + "/Saves/";

    private static readonly string SAVE_FOLDER = /*Application.isEditor ? SAVE_FOLDER_EDITOR : */STANDALONE_SAVE_FOLDER;
    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static string Save(string saveString, string saveName = "")
    {
        string trueName = "";
        int saveNumber = 1;

        if (string.IsNullOrEmpty(saveName))
        {
            while (File.Exists(SAVE_FOLDER + "save_" + saveNumber + ".json"))
                saveNumber++;
            trueName = "save_" + saveNumber;
        }
        else
        {
            if (!File.Exists(SAVE_FOLDER + saveName + ".json"))
                trueName = saveName;
            else
            {
                while (File.Exists(SAVE_FOLDER + saveName + "(" + saveNumber + ").json"))
                    saveNumber++;
                trueName = saveName + "(" + saveNumber + ")";
            }
        }
        File.WriteAllText(SAVE_FOLDER + trueName + ".json", saveString);

        return (SAVE_FOLDER + trueName + ".json");
    }

    public static string Load()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        //FileInfo[] saveFiles = directoryInfo.GetFiles();
        FileInfo mostRecentFile = null;

        GameObject scene = null;



        if (Manager.Instance != null)
            scene = Manager.Instance.sceneSelected;

        if (scene)
        {
            FileInfo[] saveFiles = SaveSystem.GetSaveFiles();

            foreach (FileInfo fileInfo in saveFiles)
            {
                if (scene.GetComponent<SceneFileScr>().nameTag.Contains(fileInfo.Name))
                {
                    if (!scene.GetComponent<SceneFileScr>().nameTag.Contains("meta"))
                    {
                        mostRecentFile = fileInfo;
                    }
                }
            }

            
        }



        //foreach (FileInfo fileInfo in saveFiles)
        //{
        //    if (mostRecentFile == null)
        //    {
        //        mostRecentFile = fileInfo;
        //    }
        //    else
        //    {
        //        if (fileInfo.LastWriteTime > mostRecentFile.LastWriteTime)
        //        {
        //            mostRecentFile = fileInfo;
        //        }
        //    }
        //}

        if (mostRecentFile != null)
        {
            string saveString = File.ReadAllText(SAVE_FOLDER + mostRecentFile.Name);
            return saveString;
        }
        return null;
    }

    public static FileInfo[] GetSaveFiles()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
        FileInfo[] saveFiles = directoryInfo.GetFiles();

        return saveFiles;
    }
}
