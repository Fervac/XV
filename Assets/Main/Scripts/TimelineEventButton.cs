using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TimelineEventButton : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float start;
    public float end;

    public float duration;

    private int type;

    public Text indicator;

    private GameObject limitL;
    private GameObject limitR;

    private Action _event;
    public Action action { get { return _event; } }
    private ActionActor actor;

    void Start()
    {
        limitL = this.transform.parent.Find("Limits/LL").gameObject;
        limitR = this.transform.parent.Find("Limits/RL").gameObject;
    }

    public TimelineEventButton(int _type, float _start, float _end, float _duration, Action action, ActionActor actor, string indi = "A")
    {
        this.type = _type;

        this.start = _start;
        this.end = _end;
        this.duration = _duration;

        indicator.text = indi;

        this._event = action;
        this.actor = actor;

        Resize();
        UpdatePositionByTime();
    }

    public void SetProperties(int _type, float _start, float _end, float _duration, Action action, ActionActor actor, string indi = "A")
    {
        this.type = _type;

        this.start = _start;
        this.end = _end;
        this.duration = _duration;

        indicator.text = indi;

        this._event = action;
        this.actor = actor;

        Resize();
        UpdatePositionByTime();
    }

    private void Resize()
    {
        float lineWidth = this.transform.parent.transform.parent.GetComponent<TimelineEvent>().timelineObject.GetComponent<RectTransform>().sizeDelta.x;
        float durValue = (this.duration) / Manager.Instance.timeline.duration;
        if (durValue <= 0)
        {
            print("Error, bad bad");
            return;
        }
        this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(lineWidth * (durValue), 30);
    }

    public void Dispose()
    {
        this.transform.parent.transform.parent.GetComponent<TimelineEvent>().DeleteEvent(this.gameObject, _event);
        GameObject.Destroy(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Manager.Instance.eventDetailPanel.SetPanel(_event, this);
        }
        if (eventData.button == PointerEventData.InputButton.Right)
            Dispose();
    }

    private Vector3 position;

    private void UpdatePositionByTime()
    {
        if (!limitL || !limitR)
        {
            limitL = this.transform.parent.Find("Limits/LL").gameObject;
            limitR = this.transform.parent.Find("Limits/RL").gameObject;
        }
        double L = System.Math.Round(limitL.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);
        double R = System.Math.Round(limitR.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);

        float width = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;

        float lineWidth = this.transform.parent.transform.parent.GetComponent<TimelineEvent>().timelineObject.GetComponent<RectTransform>().sizeDelta.x;
        float durValue = (1) / Manager.Instance.timeline.duration;
        double startPos = (lineWidth * durValue) * start;
       
        this.transform.position = new Vector3((float)startPos + limitL.transform.position.x, this.transform.position.y, this.transform.position.z);
    }

    private bool UpdateTimeParameters()
    {
        double cur = System.Math.Round(transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);

        double L = System.Math.Round(limitL.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);
        double R = System.Math.Round(limitR.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);

        // Round the cur value and fixing the position (else we might have several very little variation for the same cur value
        cur = System.Math.Round(cur);
        transform.position = new Vector3((float)(cur + System.Math.Round(limitL.transform.position.x, 2)), transform.position.y, transform.position.z);

        double startTime = (cur / R) * Manager.Instance.GetDuration();
        startTime = System.Math.Round(startTime, 2);

        if (this.start == (float)startTime)
            return false;
        this.start = (float)startTime;
        this.end = (float)(startTime + duration);
        return true;
    }

    private GameObject toolTip = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Dispose();
            return;
        }
        position = transform.position;
        transform.SetAsLastSibling();
        // Display startTime tooltip
        toolTip = Manager.Instance.GetEventTooltip();
        toolTip.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + this.gameObject.GetComponent<RectTransform>().sizeDelta.y, this.transform.position.z);
        toolTip.SetActive(true);
    }

    /*
     * Drag the event.
     * Should also modify this.start and this.end variable to match timeline
     * ------------
     * Should check for overlap and forbid overlap
     * To check for overlap, we need to check if _event.start is inferior to other action.end and that _event.
     */
    public void OnDrag(PointerEventData data)
    {
        float width = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        Vector3 np = new Vector3(data.position.x, transform.position.y, 0);
        Vector3 save = transform.position;
        if (np.x <= limitL.transform.position.x)
            np.x = limitL.transform.position.x;
        else if (np.x >= limitR.transform.position.x - width)
            np.x = limitR.transform.position.x - width;

        transform.position = np;
        // Update time parameters and check overlap
        if (!UpdateTimeParameters())
            transform.position = save;

        // Set toolTip position and text
        toolTip.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + this.gameObject.GetComponent<RectTransform>().sizeDelta.y, this.transform.position.z);
        toolTip.GetComponentInChildren<Text>().text = Timeline.timeToString(this.start);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float width = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        Vector3 np = new Vector3(eventData.position.x, transform.position.y, 0);
        Vector3 save = transform.position;
        if (np.x <= limitL.transform.position.x)
            np.x = limitL.transform.position.x;
        else if (np.x >= limitR.transform.position.x - width)
            np.x = limitR.transform.position.x - width;

        transform.position = np;
        // Update time parameters and check overlap
        if (!UpdateTimeParameters())
            transform.position = save;

        // Update action and sort actions list by start time
        _event.start = start;
        _event.end = end;
        actor.SortActions();
        actor.UpdateActions();

        //Manager.Instance.timeline.UpdateCursor(_event.start / Manager.Instance.timeline.duration);

        // Disable startTime tooltip
        GameObject toolTip = Manager.Instance.GetEventTooltip();
        toolTip.SetActive(false);
    }

    public void UpdateEvent()
    {
        this.start = _event.start;
        this.end = _event.end;
        if (_event.duration != this.duration)
            Resize();
        this.duration = _event.duration;
        //print(start + " " + end + " " + duration);

        UpdatePositionByTime();
    }
}
