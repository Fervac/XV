using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimelineEventDetailPanel : MonoBehaviour
{
    public TimelineEventButton _event;
    public Action current;

    public InputField AnimName;
    public InputField AnimNotes;
    public InputField AnimST;
    public InputField AnimET;
    public InputField AnimDT;

    private bool startEdit = false;
    private bool endEdit = false;
    private bool durationEdit = false;

    public void SetPanel(Action action, TimelineEventButton button)
    {
        _event = button;
        current = action;

        AnimName.text = current.name;
        AnimNotes.text = current.notes;

        startEdit = true;
        endEdit = true;
        durationEdit = true;
        AnimST.text = current.start.ToString();
        AnimET.text = current.end.ToString();
        AnimDT.text = current.duration.ToString();
        startEdit = false;
        endEdit = false;
        durationEdit = false;
    }

    public void UpdateName(string name)
    {
        current.name = name;
    }

    public void UpdateNotes(string notes)
    {
        current.notes = notes;
    }

    public void UpdateStart(string start)
    {
        if (string.IsNullOrWhiteSpace(start) || string.IsNullOrEmpty(start) || endEdit || durationEdit)
            return;
        startEdit = true;
        float _start = current.start;
        try
        {
            _start = float.Parse(start);
            if (_start > Manager.Instance.timeline.duration - current.duration)
            {
                /*AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();*/
                return;
            }
            else
            {
                current.start = _start;
                current.end = _start + current.duration;
                AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();
                AnimDT.text = current.duration.ToString();
                // Then callback to button event to update time/duration
                _event.UpdateEvent();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            print("Not happy");
        }
    }

    public void UpdateEnd(string end)
    {
        if (string.IsNullOrWhiteSpace(end) || string.IsNullOrEmpty(end) || startEdit || durationEdit)
            return;
        endEdit = true;
        float _end = current.end;
        try
        {
            _end = float.Parse(end);
            if (_end > Manager.Instance.timeline.duration)
            {
                /*AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();*/
                return;
            }
            else
            {
                current.end = _end;
                current.start = _end - current.duration;
                AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();
                AnimDT.text = current.duration.ToString();
                // Then callback to button event to update time/duration
                _event.UpdateEvent();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            print("Not happy");
        }
    }

    public void UpdateDuration(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration) || string.IsNullOrEmpty(duration) || startEdit || endEdit)
            return;
        durationEdit = true;
        float _duration = current.duration;
        try
        {
            _duration = float.Parse(duration);
            if (_duration > Manager.Instance.timeline.duration)
            {
              /*  AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();
                AnimDT.text = current.duration.ToString();*/
                return;
            }
            else
            {
                current.duration = _duration;
                current.end = current.start + current.duration;
                AnimST.text = current.start.ToString();
                AnimET.text = current.end.ToString();
                AnimDT.text = current.duration.ToString();
                // Then callback to button event to update time/duration
                print(_event);
                _event.UpdateEvent();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            print("Not happy");
        }
    }

    public void UpdateStartEnd()
    {
        startEdit = false;
    }

    public void UpdateEndEnd()
    {
        endEdit = false;
    }

    public void UpdateDurationEnd()
    {
        durationEdit = false;
    }
}
