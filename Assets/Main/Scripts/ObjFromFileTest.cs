using Dummiesman;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;

public class ObjFromFileTest : MonoBehaviour
{
    string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;

    public Text errorText;
    public Transform content;
    public GameObject contentPrefab;
    public Sprite notFound;

    public void OpenExplorer()
    {
        //objPath = EditorUtility.OpenFilePanel("Object to import", "", "obj");
        objPath = string.Empty;
    }

    public void LoadObject()
    {
        if (!File.Exists(objPath))
        {
            error = "File doesn't exist.";
        }
        else
        {
            loadedObject = new OBJLoader().Load(objPath);

            GameObject tmp = Manager.Instance.AddToLoadedList(loadedObject);

            AddToScrollview(tmp);

            error = string.Empty;

            loadedObject.transform.position = new Vector3(0, 800, 0);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            errorText.color = Color.red;
            StartCoroutine(ErrorCoroutine());
        }
    }

    IEnumerator ErrorCoroutine()
    {
        errorText.text = error;

        yield return new WaitForSeconds(2);

        errorText.text = "";
    }

    public void AddToScrollview(GameObject prefab)
    {
        GameObject tmp = Instantiate(contentPrefab);
        tmp.transform.SetParent(content);
        tmp.GetComponent<DragHandler>().prefab = prefab;
        tmp.GetComponent<Image>().sprite = notFound;
    }
}
