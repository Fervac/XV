using Dummiesman;
using System.Collections;
using System.IO;
//using UnityEditor;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using AsImpL;
using UnityEngine.EventSystems;

public class ObjFromFileTest : MonoBehaviour
{
    public string objPath = string.Empty;
    string error = string.Empty;
    GameObject loadedObject;

    public Text errorText;
    public Transform content;
    public GameObject contentPrefab;
    public Sprite notFound;

    [SerializeField]
    private string objectName = "MyObject";
    [SerializeField]
    private ImportOptions importOptions = new ImportOptions();

    [SerializeField]
    private PathSettings pathSettings;

    private ObjectImporter objImporter;

    public void OpenExplorer()
    {
        objPath = string.Empty;


        FileBrowser.SetFilters(false, new FileBrowser.Filter(".obj", ".obj"));

        FileBrowser.SetDefaultFilter(".obj");

        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe", ".fbx", ".png", ".blend", ".txt");

        //FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        //EventSystem[] sceneEventSystems = FindObjectsOfType<EventSystem>();

        //foreach (EventSystem evnt in sceneEventSystems)
        //{
        //    evnt.gameObject.AddComponent<StandaloneInputModule>();
        //}

        //Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            //for (int i = 0; i < FileBrowser.Result.Length; i++)
                //Debug.Log(FileBrowser.Result[i]);

            // Read the bytes of the first file via FileBrowserHelpers
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            string destinationPath = Path.Combine(FileBrowserHelpers.GetDirectoryName(FileBrowser.Result[0]), FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            objPath = destinationPath;

        }
    }

    [System.Obsolete]
    public void LoadObject()
    {
        if (!File.Exists(objPath))
        {
            error = "File doesn't exist.";
        }
        else
        {
            if (!File.Exists(Application.persistentDataPath + "/UserImports/" + (FileBrowserHelpers.GetFilename(objPath))))
            {
                File.Copy(objPath, Path.Combine(Application.persistentDataPath + "/UserImports/", FileBrowserHelpers.GetFilename(objPath)));
            }

            loadedObject = new GameObject();
            objImporter = loadedObject.GetComponent<ObjectImporter>();
            if (objImporter == null)
            {
                objImporter = loadedObject.AddComponent<ObjectImporter>();
            }

            objImporter.ImportModelAsync(objectName, objPath, loadedObject.transform, importOptions);

            //loadedObject = new OBJLoader().Load(objPath);

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
