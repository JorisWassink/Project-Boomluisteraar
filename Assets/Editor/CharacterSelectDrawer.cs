using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CharacterSelectAttribute))]
public class CharacterSelectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        var db = prop.serializedObject.targetObject as DialogueDataBase;
        if (db == null || prop.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(pos, prop, label);
            return;
        }

        var ids = new List<string>();
        var names = new List<string>();
        foreach (var c in db.characterEntries)
        {
            ids.Add(c.treeId);
            names.Add(string.IsNullOrEmpty(c.characterName) ? c.characterEntry.treeName : c.characterName);
        }

        int current = ids.IndexOf(prop.stringValue);
        EditorGUI.BeginChangeCheck();
        int picked = EditorGUI.Popup(pos, label.text, current, names.ToArray());
        if (EditorGUI.EndChangeCheck())
            prop.stringValue = ids[picked];
    }
}