using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour
{

    public List<GameObject> items;
    public GameObject mount;

    public bool isMoving { get; set; }
    public bool isRotating { get; set; }
    public bool isTaking { get; set; }
    public bool isUsing { get; set; }

    public Action current;

    private float moveDelta = 0.0f;
    private float takeDelta = 0.0f;

    void Start()
    {
        isMoving = false;
        isRotating = false;
        isTaking = false;
        isUsing = false;

        current = null;
        mount = null;
        items = new List<GameObject>();
    }

    /*
     * Function used to dispatch action behavior. 
     */
    public void DoAction(Action action)
    {
        switch (action.type)
        {
            case actionType.MOVE:
                isMoving = true;
                isTaking = false;
                break;
            case actionType.ROTATE:
                isRotating = true;
                break;
            case actionType.TAKE:
                isTaking = true;
                isUsing = false;
                break;
            case actionType.USE:
                isUsing = true;
                isTaking = false;
                break;
            default:
                return;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            moveDelta += Time.deltaTime;
            Vector3 position = Vector3.Lerp(current.start_pos, current.end_pos, moveDelta / current.duration);
            // Animate model

            this.transform.position = position;
            // Move stuff (and rotate stuff ?)
            if (moveDelta >= current.duration)
            {
                moveDelta = 0.0f;
                isMoving = false;
            }
        }
        if (isTaking)
        {
            takeDelta += Time.deltaTime;
            // Animate model

            // Take stuff
            if (takeDelta >= current.duration)
            {
                items.Add(current.object_target);
                current.object_target.transform.localScale = new Vector3(0, 0, 0); // TODO : find other way to hide it | Maybe just `activeSelf = false`
                takeDelta = 0.0f;
                isTaking = false;
            }
        }
        if (isUsing)
        {
            // Use stuff
        }
    }
}