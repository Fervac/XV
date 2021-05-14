using Dummiesman;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ObjFromFileTest : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;

    //void OnGUI() {
    //    objPath = GUI.TextField(new Rect(100, 100, 256, 32), objPath);

    //    GUI.Label(new Rect(100, 100, 256, 32), "Obj Path:");
    //    if(GUI.Button(new Rect(356, 132, 64, 32), "Load File"))
    //    {
    //        //file path
    //        if (!File.Exists(objPath))
    //        {
    //            error = "File doesn't exist.";
    //        }else{
    //            if(loadedObject != null)            
    //                Destroy(loadedObject);
    //            loadedObject = new OBJLoader().Load(objPath);
    //            error = string.Empty;
    //        }
    //    }

    //    if(!string.IsNullOrWhiteSpace(error))
    //    {
    //        GUI.color = Color.red;
    //        GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
    //        GUI.color = Color.white;
    //    }
    //}

    public void LoadObject()
    {
        objPath = GetComponentInChildren<InputField>().text;

        if (!File.Exists(objPath))
        {
            error = "File doesn't exist.";
        }
        else
        {
            if (loadedObject != null)
                Destroy(loadedObject);
            loadedObject = new OBJLoader().Load(objPath);
            error = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            GUI.color = Color.red;
            GUI.Box(new Rect(0, 64, 256 + 64, 32), error);
            GUI.color = Color.white;
        }
    }
}
