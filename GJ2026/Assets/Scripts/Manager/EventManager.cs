using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, Action> events = new Dictionary<string, Action>();

    public void Subscribe(string eventName, Action listener)
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName] += listener;
        }
        else
        {
            events[eventName] = listener;
        }
    }

    public void Unsubscribe(string eventName, Action listener)
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName] -= listener;
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName]?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Event not found: {eventName}");
        }
    }
}
