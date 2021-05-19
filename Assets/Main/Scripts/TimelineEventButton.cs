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

    void Start()
    {
        limitL = this.transform.parent.Find("Limits/LL").gameObject;
        limitR = this.transform.parent.Find("Limits/RL").gameObject;
    }

    public TimelineEventButton(int _type, float _start, float _end, float _duration, Action action, string indi = "A")
    {
        this.type = _type;

        this.start = _start;
        this.end = _end;
        this.duration = _duration;

        indicator.text = indi;

        this._event = action;
    }

    public void SetProperties(int _type, float _start, float _end, float _duration, Action action, string indi = "A")
    {
        this.type = _type;

        this.start = _start;
        this.end = _end;
        this.duration = _duration;

        indicator.text = indi;

        this._event = action;

        // Should modify position in accordance of _start
    }

    public void Dispose()
    {
        this.transform.parent.transform.parent.GetComponent<TimelineEvent>().DeleteEvent(this.gameObject, _event);
        GameObject.Destroy(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
        /*if (eventData.button == PointerEventData.InputButton.Left)
            Debug.Log("Left click"); // Depend on where the input is, right mean change duration from right else from left or if in middle move event
        else */
        if (eventData.button == PointerEventData.InputButton.Right)
            Dispose();
    }

    private Vector3 position;

    private void UpdateTimeParameters()
    {
        double cur = System.Math.Round(transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);

        double L = System.Math.Round(limitL.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);
        double R = System.Math.Round(limitR.transform.position.x, 2) - System.Math.Round(limitL.transform.position.x, 2);

        double startTime = (cur / R) * Manager.Instance.GetDuration();
        this.start = (float)startTime;
        this.end = (float)(end + startTime);
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
     */
    public void OnDrag(PointerEventData data)
    {
        float width = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        Vector3 np = new Vector3(data.position.x, transform.position.y, 0);
        if (np.x <= limitL.transform.position.x)
            np.x = limitL.transform.position.x;
        else if (np.x >= limitR.transform.position.x - width)
            np.x = limitR.transform.position.x - width;

        transform.position = np;
        // Update time parameters
        UpdateTimeParameters();
        // Set toolTip position and text
        toolTip.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + this.gameObject.GetComponent<RectTransform>().sizeDelta.y, this.transform.position.z);
        toolTip.GetComponentInChildren<Text>().text = Timeline.timeToString(this.start);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float width = this.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        Vector3 np = new Vector3(eventData.position.x, transform.position.y, 0);
        if (np.x <= limitL.transform.position.x)
            np.x = limitL.transform.position.x;
        else if (np.x >= limitR.transform.position.x - width)
            np.x = limitR.transform.position.x - width;
        transform.position = np;

        // Disable startTime tooltip
        GameObject toolTip = Manager.Instance.GetEventTooltip();
        toolTip.SetActive(false);
    }
}
