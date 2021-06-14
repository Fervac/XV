using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum actionType
{
    MOVE,
    TAKE,
    PUT,
    USE,
    ROTATE
}

#region Action Classes

[System.Serializable]
public class Action
{
    private int index;

    public float duration;

    public float start;
    public float end;

    public actionType type;

    public GameObject object_operator;
    public int op_iid = 0;
    public GameObject object_target;
    public int tar_iid = 0;

    public Vector3 start_pos;
    public Vector3 end_pos;

    public Vector3 target_pos;
    public Vector3 target_rot;

    public Vector3 start_forward;
    public Vector3 end_forward;

    public float angle = 0.0f;

    public string name = "";
    public string notes = "";

    public bool umount = false;

    #region Action Declaration

    public Action(int index, float duration, float start, float end, actionType type, GameObject object_operator, GameObject object_target = null)
    {
        this.index = index;

        this.duration = duration;
        this.start = start;
        this.end = end;

        this.type = type;

        this.object_operator = object_operator;
        if (object_operator)
            this.op_iid = object_operator.GetInstanceID();
        this.object_target = object_target;
        if (object_target)
            this.tar_iid = object_target.GetInstanceID();

        this.name = GetActionFullName(type);
    }

    public Action(int index, float duration, float start, float end, actionType type, GameObject object_operator, GameObject object_target,
        Vector3 start_pos, Vector3 end_pos)
    {
        this.index = index;

        this.duration = duration;
        this.start = start;
        this.end = end;
        
        this.type = type;

        this.object_operator = object_operator;
        if (object_operator)
            this.op_iid = object_operator.GetInstanceID();
        this.object_target = object_target;
        if (object_target)
            this.tar_iid = object_target.GetInstanceID();

        this.start_pos = start_pos;
        this.end_pos = end_pos;

        Vector3 dir = Vector3.Normalize(end_pos - start_pos);
        this.angle = Vector3.SignedAngle(object_operator.transform.forward, dir, new Vector3(0, 1, 0));

        this.name = GetActionFullName(type);
    }

    public Action(int index, float duration, float start, float end, actionType type, GameObject object_operator, GameObject object_target,
        Vector3 start_pos, Vector3 end_pos, Vector3 start_forward, Vector3 end_forward)
    {
        this.index = index;

        this.duration = duration;
        this.start = start;
        this.end = end;

        this.type = type;

        this.object_operator = object_operator;
        if (object_operator)
            this.op_iid = object_operator.GetInstanceID();
        this.object_target = object_target;
        if (object_target)
            this.tar_iid = object_target.GetInstanceID();

        this.start_pos = start_pos;
        this.end_pos = end_pos;

        this.start_forward = start_forward;
        this.end_forward = end_forward;

        Vector3 dir = Vector3.Normalize(end_pos - start_pos);
        this.angle = Vector3.SignedAngle(object_operator.transform.forward, dir, new Vector3(0, 1, 0));

        this.name = GetActionFullName(type);
    }

    public Action(Action act)
    {
        index = act.index;
        
        duration = act.duration;

        start = act.start;
        end = act.end;

        type = act.type;

        object_operator = act.object_operator;
        op_iid = act.op_iid;
        object_target = act.object_target;
        tar_iid = act.tar_iid;

        start_pos = act.start_pos;
        end_pos = act.end_pos;

        target_pos = act.target_pos;
        target_rot = act.target_rot;

        start_forward = act.start_forward;
        end_forward = act.end_forward;

        angle = act.angle;

        name = act.name;
        notes = act.notes;

        umount = act.umount;
    }

    #endregion

    public static string GetActionName(actionType type)
    {
        string name = "";
        switch (type)
        {
            case actionType.MOVE:
                name = "M";
                break;
            case actionType.ROTATE:
                name = "R";
                break;
            case actionType.TAKE:
                name = "I";
                break;
            case actionType.PUT:
                name = "P";
                break;
            case actionType.USE:
                name = "U";
                break;
            default:
                name = "E";
                break;
        }
        return name;
    }

    public static string GetActionFullName(actionType type)
    {
        string name = "";
        switch (type)
        {
            case actionType.MOVE:
                name = "Move";
                break;
            case actionType.ROTATE:
                name = "Rotate";
                break;
            case actionType.TAKE:
                name = "Interaction";
                break;
            case actionType.USE:
                name = "Use";
                break;
            default:
                name = "Error";
                break;
        }
        return name;
    }

    public static int SortByStartTime(Action a1, Action a2)
    {
        return a1.start.CompareTo(a2.start);
    }

    public void UpdateTransform(Vector3 startpos, Vector3 endForward)
    {
        Vector3 dir = Vector3.Normalize(this.end_pos - startpos);

        Vector3 euler = this.object_operator.transform.eulerAngles;
        this.object_operator.transform.eulerAngles = endForward;

        this.angle = Vector3.SignedAngle(this.object_operator.transform.forward, dir, new Vector3(0, 1, 0));
        this.object_operator.transform.eulerAngles = euler;
        Vector3 endEuler = new Vector3(endForward.x, endForward.y + angle, endForward.z);

        this.start_forward = endForward;
        this.end_forward = endEuler;

        this.start_pos = startpos;

        if (this.type == actionType.USE && this.umount)
            this.end_pos = startpos;
        //Debug.Log(this.type + ":" + this.start_pos + " -> " + this.end_pos);
    }
}

public class ActionActor
{
    public GameObject object_operator;
    public int actionCount;

    public List<Action> actions;

    public Vector3 position;
    public Vector3 rotation;

    public ActionActor(GameObject object_operator, int actionCount)
    {
        this.object_operator = object_operator;
        this.actionCount = actionCount;

        this.actions = new List<Action>();

        position = object_operator.transform.position;
        rotation = object_operator.transform.eulerAngles;
    }

    public void AddAction(Action action)
    {
        actions.Add(action);
        actionCount += 1;
    }

    public void DeleteAction(Action action)
    {
        actions.Remove(action);
        actionCount -= 1;
    }

    public void SortActions()
    {
        this.actions.Sort(Action.SortByStartTime);
    }

    public void UpdateAction(Action action)
    {
        int index = actions.FindIndex(x => x == action);
        Debug.Log(index);
        if (index != -1)
        {
            Debug.Log(action.start + " -> " + action.end);
            Debug.Log(actions[index].start + " -> " + actions[index].end);
        }
    }

    public void ResetTransform()
    {
        this.object_operator.transform.position = position;
        this.object_operator.transform.eulerAngles = rotation;
    }

    public void UpdateActions()
    {
        int i = -1;

        /*if (actions.Count >= 1) // Why ? Not sure if we need to keep it
        {
            this.object_operator.transform.position = position;//actions[0].start_pos;
            this.object_operator.transform.eulerAngles = rotation;
        }*/

        foreach (Action act in actions) // Update to check for all actions in which the target or operator is the current actor
        {
            if (act == actions[0])
            {
                act.UpdateTransform(position, rotation);
                Manager.Instance.timeline.GetTargetPosition(act, this);
                continue;
            }
            ++i;
            //Manager.Instance.timeline.PlayUntil(act.end); Not useful as it's not modifying the time cursor which is used in model manager ;(
            //Debug.Log(actions[i].type + " : " + actions[i].end_pos);
            act.UpdateTransform(actions[i].end_pos, actions[i].end_forward);
            Manager.Instance.timeline.GetTargetPosition(act, this);
        }
    }

    /*public bool isObjectTargeted(GameObject target)
    {
        if (actionCount < 1)
            return false;
        foreach (Action act in actions)
        {
            if (act.object_target == target)
                return true;
        }
        return false;
    }*/
}

#endregion

public class Timeline : MonoBehaviour
{
    public float duration = 30f;

    public List<Action> actions;        // All actions present in the timeline will be listed here // For what reason ?
    public List<ActionActor> objects;

    private List<GameObject> objects_event;

    public GameObject eventPrefab;
    public GameObject contentParent;

    public float timeCursor { get; set; }
    public Text timeCursorIndicator;
    public Slider timeCursorObject;
    public InputField timeDuration;

    private Text CurrentTime;
    private Text TotalTime;

    private bool isPlaying = false;
    private bool isRecording = false;
    private bool tooltip = false;

    public List<Color> eventStyle = new List<Color>();

    void Start()
    {
        actions = new List<Action>();

        objects = new List<ActionActor>();
        objects_event = new List<GameObject>();
        timeCursor = 0.0f;
        isPlaying = false;

        timeDuration.text = duration.ToString();

        this.CurrentTime = transform.Find("TimelineOverview/TimeView/CurrentTime").gameObject.GetComponent<Text>();
        this.TotalTime = transform.Find("TimelineOverview/TimeView/TotalTime").gameObject.GetComponent<Text>();
    }

    void Awake()
    {
        transform.Find("TimelineCursor/Handle Slide Area/Handle/ToolTip").gameObject.SetActive(tooltip);
        this.CurrentTime = transform.Find("TimelineOverview/TimeView/CurrentTime").gameObject.GetComponent<Text>();
        this.TotalTime = transform.Find("TimelineOverview/TimeView/TotalTime").gameObject.GetComponent<Text>();
        UpdateOverview();
    }

    #region Action Events

    private float GetActionStart(Action action, ActionActor actor)
    {
        float firstTimePos = action.start;
        float endTimePos = firstTimePos + action.duration;

        foreach (Action act in actor.actions)
        {
            if (act.start == firstTimePos
                || (act.start > firstTimePos && act.start < endTimePos)
                || (act.start < firstTimePos && act.end > firstTimePos))
            {
                firstTimePos = act.end;
                endTimePos = firstTimePos + action.duration;
            }
        }

        return firstTimePos;
    }

    /*
     * Function used to get correct position for the actions in ONE actor.
     */
    private Vector3 GetActionStartPos(Action action, ActionActor actor)
    {
        Vector3 startpos = actor.object_operator.transform.position;
        float firstTimePos = action.start;
        float endTimePos = firstTimePos + action.duration;
        Vector3 endForward = action.start_forward;

        actionType test = 0;

        foreach (Action act in actions)
        {
            if (act.object_operator != action.object_operator && act.object_target != action.object_operator)
                continue;
            if ((act.start == firstTimePos
                || (act.start > firstTimePos && act.start < endTimePos)
                || (act.start < firstTimePos && act.end > firstTimePos)))
            {
                firstTimePos = act.end;
                endTimePos = firstTimePos + action.duration;
                startpos = act.end_pos;
                endForward = act.end_forward;
                test = act.type;
                //print(action.type + " | " + act.type);
            }
        }
        /*print(firstTimePos);
        print(startpos + " -> " + action.end_pos + " vs " + actor.position);
        print(action.start_pos);*/
        if (firstTimePos == 0.0f)
        {
            action.start_pos = actor.position;
            action.start_forward = actor.rotation;
        }
        if (startpos != action.start_pos)
            action.UpdateTransform(startpos, endForward);
        return startpos;
    }

    /*
     * This functions is used to get the updated position of the target object for TAKE/USE actions
     */
    public void GetTargetPosition(Action action, ActionActor actor)
    {
        if (action.type != actionType.TAKE /*&& action.type != actionType.PUT*/ && action.type != actionType.USE)
            return;
        if (action.type == actionType.USE && action.umount)
            return;

        int type = 0;
        float timePos = 0.0f;
        GameObject target = action.object_target;
        Vector3 npos = action.end_pos;
        if (!target)
            return;
        foreach (Action act in actions) // Can be expensive if there is a lot of actions in the timeline (but won't impact performence at our level)
        {
            if (act.object_operator != target && act.object_target == null)
                continue;
            if (act.object_operator == target || act.object_target == target)
            {
                type = act.object_operator == target ? 0 : 1;
                if (timePos <= act.end && act.end <= action.start)
                {
                    npos = act.end_pos;
                    timePos = act.end;
                }
            }
        }
        action.end_pos = npos;
    }

    /*
     * This function is used to add action to the timeline.
     * A object can only have so many action, the whole action sequence cannot be superior to the timeline duration
     * Thus if the user try to add too many action, the addition might be refused, an error should then be display.
     * -------
     * If the object_operator of the action isn't present in the timeline, it will be added to its object list.
     * -------
     * The action will be added at the first possible slot in the timeline.
     */
    public void AddAction(Action action, GameObject object_operator)
    {
        int i = 0;

        // Check if object_operator is present in the timeline
        if (objects.Find(x => x.object_operator == object_operator) == null)
        {
            objects.Add(new ActionActor(object_operator, 0));
            GameObject new_event = Instantiate(eventPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            new_event.transform.position = new Vector3(-783, -25 - (objects_event.Count * 32), 0);
            new_event.transform.SetParent(contentParent.transform, false);
            objects_event.Add(new_event);

            new_event.GetComponent<TimelineEvent>().parent = this;
            new_event.GetComponent<TimelineEvent>().SetActor(objects[objects.Count - 1], objects.Count % 2 == 0);
            //new_event.GetComponent<TimelineEvent>().AddEvent(action);
        }

        i = objects.FindIndex(x => x.object_operator == object_operator);
        // Check if actor is not already filled with actions (total actions duration vs timeline duration)
        float actionCurrentSum = 0.0f;
        foreach (Action act in objects[i].actions)
            actionCurrentSum += act.duration;
        actionCurrentSum += action.duration;
        if (actionCurrentSum > this.duration)
        {
            print("Error, timeline is full");
            return;
        }
        else
        {
            // Check where we can place the new action
            float startTime = GetActionStart(action, objects[i]);
            if (startTime >= duration || startTime + action.duration > duration)
            {   
                print("Error, timeline is full");
                return;
            }
            //if (action.type == actionType.MOVE) // TODO : Modify, each actionType will need to have pos and rot
            /*
             * If action.type == actionType.TAKE, we should check if there is ANY action with the target object.
             * If there is, check is time is compatible, one object cannot be taken twice in the same time frame (and without being put on the ground).
             * And also update end_pos / start_pos of the action based on the target.
             */
            action.start_pos = GetActionStartPos(action, objects[i]);
            action.start = startTime;
            action.end = startTime + action.duration;
            GetTargetPosition(action, objects[i]);
        }

        // Add action to the timeline
        actions.Add(action);
        objects[i].AddAction(action);
        objects[i].SortActions();
        objects[i].UpdateActions();
        objects_event[i].GetComponent<TimelineEvent>().AddEvent(action);
    }

    /*
     * This function is used to delete an action present in the timeline.
     * Deleting the action should remove it from the timeline and,
     * if this action is the last of its object_operator, this object is deleted from the timeline.
     */
    public void DeleteAction(Action action, GameObject object_operator)
    {
        int i = objects.FindIndex(x => x.object_operator == object_operator);
        if (!actions.Contains(action))
            return;
        if (objects[i].actionCount >= 1)
        {
            if (action.type == actionType.USE && action.umount)
                objects[i].object_operator.GetComponent<ModelManager>().mountEver = action.object_target;
            actions.Remove(action);
            objects[i].DeleteAction(action);
            if (objects[i].actionCount == 0)
            {
                objects[i].ResetTransform();
                objects.RemoveAt(i);
                objects_event.RemoveAt(i);
            }
            else
            {
                objects[i].SortActions();
                objects[i].UpdateActions();
            }
        }
        else
        {
            // ERROR, should not be here
        }
    }

    private void DeleteLinkedActions(GameObject actor)
    {
        GameObject obj = null;
        List<Action> toDelete = new List<Action>();
        List<TimelineEvent> toDeleteLine = new List<TimelineEvent>();

        foreach (Action act in actions)
        {
            if (act.object_target != actor && act.object_operator != actor)
                continue;
            if (act.object_target == actor || act.object_operator == actor)
            {
                obj = objects_event.Find(x => x.GetComponent<TimelineEvent>().actor.object_operator == act.object_operator);
                if (obj)
                {
                    toDeleteLine.Add(obj.GetComponent<TimelineEvent>());
                    toDelete.Add(act);
                }
            }
        }
        for (int k = 0; k < toDelete.Count; k++)
            toDeleteLine[k].DeleteEvent(null, toDelete[k]);
    }

    public void DeleteActor(GameObject actor)
    {
        if (!actor)
            return;
        List<Action> toDelete = new List<Action>();
        List<TimelineEvent> toDeleteLine = new List<TimelineEvent>();

        int i = objects.FindIndex(x => x.object_operator == actor);
        DeleteLinkedActions(actor);
        if (i != -1)
            DeleteActor(actor);
    }

    /*
     * This functions shall delete all actions and actionActor.
     * WARNING : This does not delete objects in the scene, only their actions;
     */
    public void ClearTimeline()
    {
        GameObject eventLine = null;

        while (objects_event.Count > 0)
        {
            eventLine = objects_event[0];
            if (eventLine != null && eventLine.GetComponent<TimelineEvent>() && eventLine.GetComponent<TimelineEvent>().actor.actions.Count > 0)
                eventLine.GetComponent<TimelineEvent>().DeleteEvent(null, eventLine.GetComponent<TimelineEvent>().actor.actions[0]);
            else
            {
                print("Error should not be here");
                break;
            }
        }
    }

    #endregion

    #region Auxiliary Functions

    public static string timeToString(float value)
    {
        string str_time = "";

        int integerPart = (int)value;
        if (integerPart >= 60)
        {
            int minute = integerPart / 60;
            float second = value - (minute * 60);
            str_time = minute.ToString("0:") + second.ToString("0.00");
        }
        else
            str_time = value.ToString("0.00s");
        return str_time;
    }

    public void ToggleTooltip()
    {
        tooltip = !tooltip;
        transform.Find("TimelineCursor/Handle Slide Area/Handle/ToolTip").gameObject.SetActive(tooltip);
    }

    #endregion

    #region Parameter Update Functions

    private void UpdateTimelines()
    {
        foreach (GameObject lines in objects_event)
        {
            TimelineEvent _event = lines.GetComponent<TimelineEvent>();
            _event.UpdateTimeline();
        }
    }

    public void SetDuration(float _duration)
    {
        isPlaying = false;

        if (duration != _duration)
        {
            duration = _duration;
            timeDuration.text = duration.ToString();
            UpdateTimelines();
            UpdateOverview();
        }
    }

    public void UpdateDuration(string new_duration)
    {
        isPlaying = false;

        if (new_duration == string.Empty || int.Parse(new_duration) == 0)
        {
            timeDuration.text = duration.ToString();
            return;
        }
        if (timeCursor > float.Parse(new_duration))
            timeCursor = int.Parse(new_duration);
        float _duration = int.Parse(new_duration);
        if (duration != _duration)
        {
            duration = _duration;
            UpdateTimelines();
            UpdateCursorView();
            UpdateOverview();
        }
    }

    public void UpdateCursor(float value)
    {
        if (isPlaying)
            return;
        timeCursor = value * duration;
        if (timeCursor < 0.0f)
            timeCursor = 0.0f;
        else if (timeCursor > duration)
            timeCursor = duration;
        UpdateCursorView();
        UpdateOverview();

        // Play all actions till this point
        PlayUntil();
    }

    private void UpdateCursorView()
    {
        timeCursorIndicator.text = timeToString(timeCursor);
        timeCursorObject.value = timeCursor / duration;
    }

    private void UpdateOverview()
    {
        if (CurrentTime)
            CurrentTime.text = timeToString(timeCursor);
        if (TotalTime)
            TotalTime.text = timeToString(duration);
    }

    #endregion

    public void StartTimeline() { isPlaying = true; }
    public void PauseTimeline() { isPlaying = false; if (isRecording) StopRecording(); }
    public void StopTimeline() { isPlaying = false; timeCursor = 0.0f; UpdateCursorView(); UpdateOverview(); if (isRecording) StopRecording(); }
    public void RecordTimeline() {
        if (ManageRecording())
        {
            isRecording = !isRecording;
            isPlaying = !isPlaying;
        }
    }

    private bool ManageRecording()
    {
        if (RockVR.Video.VideoCaptureCtrl.instance.status == RockVR.Video.VideoCaptureCtrl.StatusType.NOT_START
            || RockVR.Video.VideoCaptureCtrl.instance.status == RockVR.Video.VideoCaptureCtrl.StatusType.FINISH)
        {
            RockVR.Video.VideoCaptureCtrl.instance.StartCapture();
        }
        else if (RockVR.Video.VideoCaptureCtrl.instance.status == RockVR.Video.VideoCaptureCtrl.StatusType.STARTED)
        {
            RockVR.Video.VideoCaptureCtrl.instance.StopCapture();
        }
        else if (RockVR.Video.VideoCaptureCtrl.instance.status == RockVR.Video.VideoCaptureCtrl.StatusType.STOPPED)
        {
            Debug.Log("Error : last capture is processing. Unable to capture.");
            return false;
        }
        return true;
    }

    private void StopRecording()
    {
        print("Stopping the recording");
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        isRecording = false;
        RockVR.Video.VideoCaptureCtrl.instance.StopCapture();
    }

    public bool IsPlaying() { return isPlaying; }

    private void ResetModels()
    {
        // Reset all models
        foreach (GameObject _object in Manager.Instance.loadedObjects)
        {
            ModelManager manager = _object.GetComponent<ModelManager>();
            if (manager)
                _object.GetComponent<ModelManager>().ResetModel();
        }
    }

    /*
     * This function shall execute the whole animation timeline (from current position to duration)
     */
    public void Play(bool restart = false)
    {
        if (!isPlaying)
            return;
        if (restart)
        {
            ResetModels();
            timeCursor = 0.0f;
        }

        // Launch action (if any) in the timeline
        foreach (GameObject _eventElem in objects_event)
        {
            TimelineEvent _event = _eventElem.GetComponent<TimelineEvent>();
            _event.Play(timeCursor);
        }

        if (isPlaying && timeCursor >= duration)
        {
            isPlaying = false;
            timeCursor = duration;
            if (isRecording)
                StopRecording();
            return;
        }
        timeCursor += Time.deltaTime;
        UpdateCursorView();
        UpdateOverview();
    }

    /*
     * This function shall execute the animation till the current time cursor position
     */
    public void PlayUntil(float timestamp = -1f)
    {
        if (isPlaying)
            return;

        // Replace all object at start position
        ResetModels();

        foreach (GameObject _eventElem in objects_event)
        {
            TimelineEvent _event = _eventElem.GetComponent<TimelineEvent>();
            _event.PlayUntil(timestamp != -1f ? timestamp : timeCursor);
        }
    }

    void Update() // TODO : delete
    {
        if (isPlaying) // TODO : Maybe close all popup when playing
            Play(timeCursor == duration);
        if (Input.GetKeyDown(KeyCode.Space))
            isPlaying = !isPlaying;
        if (Manager.Instance.camKaren.camMode != cameraMode.Overview)
        {
            if (Input.GetKeyDown(KeyCode.R))
                RecordTimeline();
        }
    }
}
