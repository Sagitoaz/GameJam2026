using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSequenceController : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private string[] listenEventNames = new string[] { "Button01_Pressed" };
    [SerializeField] private bool requireAllEvents = true;

    [Header("Door Sequence")]
    [SerializeField] private Door[] doors; // sắp xếp từ trên xuống
    [SerializeField] private float delayBetweenDoors = 0.2f;

    private HashSet<string> activeEvents = new HashSet<string>();
    private Coroutine sequenceCoroutine;
    private bool isOpen = false;

    private void Start()
    {
        foreach (string eventName in listenEventNames)
        {
            EventManager.Instance.Subscribe(eventName, () => OnEventPressed(eventName));
            EventManager.Instance.Subscribe(eventName + "_Released", () => OnEventReleased(eventName));
        }
    }

    private void OnDestroy()
    {
        if (EventManager.Instance == null) return;

        foreach (string eventName in listenEventNames)
        {
            EventManager.Instance.Unsubscribe(eventName, () => OnEventPressed(eventName));
            EventManager.Instance.Unsubscribe(eventName + "_Released", () => OnEventReleased(eventName));
        }
    }

    // ================= EVENT =================

    private void OnEventPressed(string eventName)
    {
        activeEvents.Add(eventName);
        CheckCondition();
    }

    private void OnEventReleased(string eventName)
    {
        activeEvents.Remove(eventName);
        CheckCondition();
    }

    private void CheckCondition()
    {
        bool shouldOpen;

        if (requireAllEvents)
            shouldOpen = activeEvents.Count == listenEventNames.Length;
        else
            shouldOpen = activeEvents.Count > 0;

        if (shouldOpen && !isOpen)
        {
            OpenSequence();
        }
        else if (!shouldOpen && isOpen)
        {
            CloseSequence();
        }
    }

    // ================= SEQUENCE =================

    private void OpenSequence()
    {
        isOpen = true;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(OpenDoors());
    }

    private void CloseSequence()
    {
        isOpen = false;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(CloseDoors());
    }

    private IEnumerator OpenDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] != null)
                doors[i].OpenDoor();

            yield return new WaitForSeconds(delayBetweenDoors);
        }
    }

    private IEnumerator CloseDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] != null)
                doors[i].CloseDoor();

            yield return new WaitForSeconds(delayBetweenDoors);
        }
    }
}
