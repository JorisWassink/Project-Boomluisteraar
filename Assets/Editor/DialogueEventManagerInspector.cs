using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CustomEditor(typeof(DialogueEventManager))]
public class DialogueEventManager_Inspector : Editor
{
    static readonly Dictionary<Scene, EventCommands[]> AllowedByScene = new()
    {
        { Scene.TitleScreen, new[] { EventCommands.UnlockPlayButton } },
    };

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var keyChoices = new List<EventCommands>();

        var commandsProp = serializedObject.FindProperty("commands");
        if (commandsProp == null || !commandsProp.isArray)
        {
            root.Add(new Label("'commands' not found or not a list on DialogueEventManager"));
            return root;
        }

        var sceneDropdown = new DropdownField("Scene",
            new List<string>(Enum.GetNames(typeof(Scene))), 0);
        var list = MakeList(commandsProp, keyChoices);

        void Refresh(Scene scene)
        {
            keyChoices.Clear();
            if (AllowedByScene.TryGetValue(scene, out var allowed))
                keyChoices.AddRange(allowed);
            list.RefreshItems();
        }

        sceneDropdown.RegisterValueChangedCallback(evt =>
            Refresh(Enum.Parse<Scene>(evt.newValue)));
        Refresh(Enum.Parse<Scene>(sceneDropdown.value));

        root.Add(sceneDropdown);
        root.Add(list);
        root.Bind(serializedObject);   // binds the ListView to its property path
        return root;
    }
    
    static ListView MakeList(SerializedProperty listProp,
                             List<EventCommands> keyChoices)
    {
        var list = new ListView
        {
            showBorder = true,
            showAddRemoveFooter = true,
            reorderable = true,
            showBoundCollectionSize = false,
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            makeItem = () =>
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.FlexStart;
                row.style.paddingTop = 2;
                row.style.paddingBottom = 2;

                var key = new DropdownField { name = "key" };
                key.style.width = 140;
                key.style.flexShrink = 0;
                key.style.marginRight = 6;

                var evt = new PropertyField { name = "event", label = "" };
                evt.style.flexGrow = 1;
                evt.style.flexShrink = 1;
                evt.style.minWidth = 0;

                row.Add(key);
                row.Add(evt);
                return row;
            },
            bindItem = (row, i) =>
            {
                var entry = listProp.GetArrayElementAtIndex(i);
                var keyProp = entry.FindPropertyRelative("key");

                // key: filtered dropdown, writes the enum's int value
                var key = row.Q<DropdownField>("key");
                key.choices = keyChoices.Select(e => e.ToString()).ToList();
                var current = keyChoices.FindIndex(e => (int)e == keyProp.intValue);
                key.SetValueWithoutNotify(current >= 0 ? keyChoices[current].ToString() : "");

                key.UnregisterCallback<ChangeEvent<string>>(OnKeyChanged);
                key.userData = (keyProp, keyChoices);
                key.RegisterCallback<ChangeEvent<string>>(OnKeyChanged);

                // value: the real UnityEvent drawer
                row.Q<PropertyField>("event")
                   .BindProperty(entry.FindPropertyRelative("action"));
            }
        };

        list.bindingPath = listProp.propertyPath;
        return list;
    }

    static void OnKeyChanged(ChangeEvent<string> evt)
    {
        var key = (DropdownField)evt.target;
        var (prop, choices) = ((SerializedProperty, List<EventCommands>))key.userData;
        if (key.index < 0 || key.index >= choices.Count) return;

        prop.intValue = (int)choices[key.index];
        prop.serializedObject.ApplyModifiedProperties();
    }
}

