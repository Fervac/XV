using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static string Save(string saveString)
    {
        int saveNumber = 1;
        while (File.Exists(SAVE_FOLDER + "save_" + saveNumber + ".txt"))
        {
            saveNumber++;
        }
        File.WriteAllText(SAVE_FOLDER + "/save_" + saveNumber + ".txt", saveString);

        return (SAVE_FOLDER + "save_" + saveNumber + ".txt");
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
            string saveString = File.ReadAllText(SAVE_FOLDER + mostRecentFile);
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
