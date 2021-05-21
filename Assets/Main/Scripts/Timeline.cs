using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum actionType
{
    MOVE,
    TAKE,
    USE,
    ROTATE
}

#region Action Classes

public class Action
{
    private int index;

    public float duration;

    public float start;
    public float end;

    public actionType type;

    public GameObject object_operator;
    public GameObject object_target;

    public Vector3 start_pos;
    public Vector3 end_pos;

    public Vector3 start_forward;
    public Vector3 end_forward;

    public float angle = 0.0f;

    #region Action Declaration

    public Action(int index, float duration, float start, float end, actionType type, GameObject object_operator, GameObject object_target = null)
    {
        this.index = index;

        this.duration = duration;
        this.start = start;
        this.end = end;

        this.type = type;

        this.object_operator = object_operator;
        this.object_target = object_target;
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
        this.object_target = object_target;
        
        this.start_pos = start_pos;
        this.end_pos = end_pos;

        Vector3 dir = Vector3.Normalize(end_pos - start_pos);
        this.angle = Vector3.SignedAngle(object_operator.transform.forward, dir, new Vector3(0, 1, 0));
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
        this.object_target = object_target;

        this.start_pos = start_pos;
        this.end_pos = end_pos;

        this.start_forward = start_forward;
        this.end_forward = end_forward;

        Vector3 dir = Vector3.Normalize(end_pos - start_pos);
        this.angle = Vector3.SignedAngle(object_operator.transform.forward, dir, new Vector3(0, 1, 0));
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
            case actionType.USE:
                name = "U";
                break;
            default:
                name = "E";
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
    }
}

public class ActionActor
{
    public GameObject object_operator;
    public int actionCount;

    public List<Action> actions;

    public ActionActor(GameObject object_operator, int actionCount)
    {
        this.object_operator = object_operator;
        this.actionCount = actionCount;

        this.actions = new List<Action>();
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

    public void UpdateActions()
    {
        int i = -1;

        if (actions.Count >= 1)
            this.object_operator.transform.position = actions[0].start_pos;

        foreach (Action act in actions)
        {
            if (act == actions[0])
                continue;
            ++i;
            act.UpdateTransform(actions[i].end_pos, actions[i].end_forward);
        }
    }
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
    private bool tooltip = false;

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
        float firstTimePos = 0.0f;
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

    private Vector3 GetActionStartPos(Action action, ActionActor actor)
    {
        Vector3 startpos = actor.object_operator.transform.position;
        float firstTimePos = 0.0f;
        float endTimePos = firstTimePos + action.duration;
        Vector3 endForward = action.start_forward;

        foreach (Action act in actor.actions)
        {
            if ((act.start == firstTimePos
                || (act.start > firstTimePos && act.start < endTimePos)
                || (act.start < firstTimePos && act.end > firstTimePos)))
            {
                firstTimePos = act.end;
                endTimePos = firstTimePos + action.duration;
                startpos = act.end_pos;
                endForward = act.end_forward;
            }
        }
        if (startpos != action.start_pos)
            action.UpdateTransform(startpos, endForward);
        return startpos;
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
            if (action.type == actionType.MOVE) // TODO : Modify, each actionType will need to have pos and rot
                action.start_pos = GetActionStartPos(action, objects[i]);
            action.start = startTime;
            action.end = startTime + action.duration;
        }

        // Add action to the timeline
        actions.Add(action);
        objects[i].AddAction(action);
        objects[i].SortActions();
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
            actions.Remove(action);
            objects[i].DeleteAction(action);
            if (objects[i].actionCount == 0)
            {
                objects.RemoveAt(i);
                objects_event.RemoveAt(i);
                if (index > 0)
                    index--;
            }
        }
        else
        {
            // ERROR, should not be here
        }
    }

    public void DeleteActor(GameObject actor)
    {
        if (!actor)
            return;
        ActionActor aa = null;
        GameObject obj = null;

        int i = objects.FindIndex(x => x.object_operator == actor);
        if (i == -1)
            return;
        aa = objects[i];
        obj = objects_event.Find(x => x.GetComponent<TimelineEvent>().actor.object_operator == actor);
        if (aa.actions.Count > 0)
            obj.GetComponent<TimelineEvent>().DeleteEvent(null, aa.actions[0]);
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

    public void SetDuration(float _duration)
    {
        isPlaying = false;

        duration = _duration;
        timeDuration.text = duration.ToString();
        UpdateOverview();
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
        duration = int.Parse(new_duration);
        UpdateCursorView();
        UpdateOverview();
    }

    public void UpdateCursor(float value)
    {
        if (isPlaying)
            return;
        timeCursor = value * duration;
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
    public void PauseTimeline() { isPlaying = false; }
    public void StopTimeline() { isPlaying = false; timeCursor = 0.0f; UpdateCursorView(); UpdateOverview(); }
    public void RecordTimeline() { print("To implement"); }

    public bool IsPlaying() { return isPlaying; }

    /*
     * This function shall execute the whole animation timeline (from current position to duration)
     */
    public void Play(bool restart = false)
    {
        if (!isPlaying)
            return;
        if (restart)
            timeCursor = 0.0f;

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
            return;
        }
        timeCursor += Time.deltaTime;
        UpdateCursorView();
        UpdateOverview();
    }

    /*
     * This function shall execute the animation till the current time cursor position
     */
    public void PlayUntil()
    {
        if (isPlaying)
            return;

        foreach (GameObject _eventElem in objects_event)
        {
            TimelineEvent _event = _eventElem.GetComponent<TimelineEvent>();
            _event.PlayUntil(timeCursor);
        }
    }

    static int k = 0;
    static int p = 1;
    static int index = 0;

    void Update() // TODO : delete
    {
        if (isPlaying)
            Play(timeCursor == duration);
        if (Input.GetKeyDown(KeyCode.Space))
            isPlaying = !isPlaying;

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject tmp = new GameObject("tmp-" + k);
            k++;
            AddAction(new Action(actions.Count, 1f, 0f, 1f, actionType.MOVE, tmp, tmp, new Vector3(0, 0, 0), new Vector3(0, 0, 0)), tmp);
        }
        if (Input.GetKeyDown(KeyCode.I) && objects_event.Count > 0)
        {
            TimelineEvent tmp = objects_event[index].GetComponent<TimelineEvent>();
            Action action;
            switch (p % 3)
            {
                case 0:
                    action = new Action(tmp.eventListCount(), 1f, 0f, 1f, actionType.MOVE, tmp.actor.object_operator);
                    break;
                case 2:
                    action = new Action(tmp.eventListCount(), 1f, 0f, 1f, actionType.TAKE, tmp.actor.object_operator);
                    break;
                default:
                    action = new Action(tmp.eventListCount(), 1f, 0f, 1f, actionType.USE, tmp.actor.object_operator);
                    break;
            }
            AddAction(action, tmp.actor.object_operator);
            p++;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            index++;
            if (index >= objects_event.Count)
                index = 0;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearTimeline();
        }
    }
}
