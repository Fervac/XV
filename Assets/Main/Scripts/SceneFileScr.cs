using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneFileScr : MonoBehaviour
{
    public SceneManager sceneManager;
    public Button btn;
    public string nameTag;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    private void Start()
    {
        Text txt = GetComponentInChildren<Text>();
        txt.text = nameTag;
    }
}
