using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueEventManager : MonoBehaviour
{
    public List<CommandEntry> commands = new();

    public void FireEvent(EventCommands key)
    {
        foreach (CommandEntry entry in commands)
        {
            if (entry.key == key)
            {
                entry.action.Invoke();
            }
        }
    }
}

[Serializable]
public class CommandEntry
{
    public EventCommands key;
    public UnityEvent action;
}