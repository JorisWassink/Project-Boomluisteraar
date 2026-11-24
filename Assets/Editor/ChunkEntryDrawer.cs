using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueDataBase.ChunkEntry))]
public class ChunkEntryDrawer : PropertyDrawer
{
    const float Spacing = 2f;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, prop);

        var isEvent = prop.FindPropertyRelative("isEvent");
        var text = prop.FindPropertyRelative("dialogueText");
        var evt = prop.FindPropertyRelative("command");
        var emotion = prop.FindPropertyRelative("emotion");

        float line = EditorGUIUtility.singleLineHeight;

        var toggleRect = new Rect(pos.x, pos.y, pos.width, line);
        var fieldRect = new Rect(pos.x, pos.y + line + Spacing, pos.width, line);
        var emotionRect = new Rect(pos.x, pos.y + (line + Spacing) * 2 , pos.width, line);

        EditorGUI.PropertyField(fieldRect, isEvent.boolValue ? evt : text, GUIContent.none);
        isEvent.boolValue = EditorGUI.ToggleLeft(toggleRect, "Is Event", isEvent.boolValue);
        if(!isEvent.boolValue) EditorGUI.PropertyField(emotionRect, emotion, GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight + Spacing;
        return prop.FindPropertyRelative("isEvent").boolValue ? line * 2 : line * 3;
    }
}