using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineEvent : MonoBehaviour
{
    public ActionActor actor;

    public Text actorName;
    public GameObject timelineObject;

    public GameObject eventPrefab;

    public bool odd;

    public Color EvenColor;
    public Color OddColor;

    private List<GameObject> eventList = new List<GameObject>();
    public int eventListCount() { return eventList.Count; }

    private int[] eventPlace;

    public Timeline parent;

    void Start()
    {
        int maxEvent = (int)(timelineObject.GetComponent<RectTransform>().sizeDelta.x / 30f);
        eventPlace = new int[maxEvent];
    }

    public void SetActor(ActionActor _actor, bool odd = false)
    {
        this.actor = _actor;
        actorName.text = actor.object_operator.name;
        this.odd = odd;
        timelineObject.GetComponent<Image>().color = odd ? OddColor : EvenColor;
    }

    /*
     * AddEvent is used to add new action to the current TimelineEvent.
     * We use ONE TimelineEvent by actor, each timeline having up to X action(s) (actions duration / main timeline duration).
     * When adding an event, we will instantiate a eventButton, used to see when the action will start and end.
     */
    public void AddEvent(Action action)
    {
        float prefabWidth = eventPrefab.GetComponent<RectTransform>().sizeDelta.x;
        float lineWidth = timelineObject.GetComponent<RectTransform>().sizeDelta.x;
        int maxNumOfEventPossible = (int)(parent.duration);
        prefabWidth = lineWidth / maxNumOfEventPossible;
        //prefabWidth = 25f;

        GameObject new_event = Instantiate(eventPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        new_event.transform.SetParent(timelineObject.transform, false);
        new_event.GetComponent<RectTransform>().sizeDelta = new Vector2(prefabWidth, 30);
        eventList.Add(new_event);
        new_event.GetComponent<TimelineEventButton>().SetProperties((int)(action.type), action.start, action.end, action.duration, action, actor, Action.GetActionName(action.type));
    }

    /*
     * DeleteEvent should handle the removal of the specified _event from the eventButton.
     * The function will clean its eventList by removing the specified eventButton, it will also delete the _event from the main timeline.
     */
    public void DeleteEvent(GameObject eventButton, Action _event)
    {
        parent.DeleteAction(_event, _event.object_operator);
        eventList.Remove(eventButton);
        if (eventList.Count == 0)
            DeleteActor();
    }

    public void DeleteActor()
    {
        GameObject.Destroy(this.gameObject);
    }

    /*
     * Update event buttons width and position when the timeline duration change
     */
    public void UpdateTimeline()
    {
    }

    /*
     * How to check where we can place the next event button ?
     * -> Make array of rect->width / width of event button ? and fill the array based on the position of the event buttons
     */

    public void Play(float timeCursor)
    {
        List<Action> actions = actor.actions;
    }
}
