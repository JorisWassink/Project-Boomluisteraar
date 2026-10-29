using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TreeWorldMap))]
public class WorldMapTree_Inspector : Editor
{
    const string ModelChildName = "TreeModel_Instance";
    const string ModelParentName = "Model";

    TextField _idField;
    TextField _sensorTypeField;
    
    Transform ModelParent => Script.transform.Find(ModelParentName);
    
    TreeWorldMap Script => (TreeWorldMap)target;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        var choices = StaticVariables.TreeIdDictionary.Values
            .Where(v => v.Item2 != StaticVariables.DataType.SoilSensor)
            .Select(v => v.Item1)
            .ToList();

        var treeIdProp = serializedObject.FindProperty("treeId");

        var dropdown = new DropdownField("Tree", choices, 0);
        dropdown.BindProperty(treeIdProp);
        dropdown.SetEnabled(!IsPrefabContext());
        dropdown.RegisterValueChangedCallback(evt => OnTreeSelected(evt.newValue));

        _idField = MakeReadOnlyField("ID");
        _sensorTypeField = MakeReadOnlyField("Sensor Type");

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(MakeDivider());
        root.Add(dropdown);
        root.Add(_idField);
        root.Add(_sensorTypeField);
        root.Add(MakeDivider());
        root.Add(BuildModelsList(choices));

        // read from the property, the dropdown's own value isn't bound yet
        var initial = choices.Contains(treeIdProp.stringValue) ? treeIdProp.stringValue : choices[0];
        OnTreeSelected(initial);

        return root;
    }

    void OnTreeSelected(string treeName)
    {
        var entry = StaticVariables.TreeIdDictionary
            .FirstOrDefault(kv => kv.Value.Item1 == treeName);
        if (entry.Key == null) return;

        _idField.SetValueWithoutNotify(entry.Key);
        _sensorTypeField.SetValueWithoutNotify(entry.Value.Item2.ToString());

        if (IsPrefabContext()) return;

        serializedObject.Update();
        serializedObject.FindProperty("id").stringValue = entry.Key;
        serializedObject.ApplyModifiedProperties();

        var go = Script.gameObject;
        if (go.name != treeName)
        {
            Undo.RecordObject(go, "Rename tree");
            go.name = treeName;
        }

        SpawnModel(treeName);
    }

    void SpawnModel(string treeName)
    {
        var parent = ModelParent;
        if (parent == null)
        {
            Debug.LogWarning($"'{Script.name}' has no child called '{ModelParentName}'.", Script);
            return;
        }

        var prefab = FindPrefab(treeName);
        var existing = parent.Find(ModelChildName);

        // already showing the right model, don't respawn
        if (existing != null && prefab != null &&
            PrefabUtility.GetCorrespondingObjectFromSource(existing.gameObject) == prefab)
            return;

        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);

        if (prefab == null) return;

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        inst.name = ModelChildName;
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        Undo.RegisterCreatedObjectUndo(inst, "Spawn tree model");
    }

    GameObject FindPrefab(string treeName)
    {
        var list = serializedObject.FindProperty("models");
        for (int i = 0; i < list.arraySize; i++)
        {
            var el = list.GetArrayElementAtIndex(i);
            if (el.FindPropertyRelative("treeName").stringValue == treeName)
                return el.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
        }
        return null;
    }

    VisualElement BuildModelsList(List<string> names)
    {
        var list = serializedObject.FindProperty("models");
        SyncModels(list, names);

        var foldout = new Foldout { text = "Models", value = true };
        for (int i = 0; i < list.arraySize; i++)
        {
            var field = new ObjectField(names[i])
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false
            };
            field.BindProperty(list.GetArrayElementAtIndex(i).FindPropertyRelative("prefab"));
            foldout.Add(field);
        }
        return foldout;
    }

    // one entry per tree name, keeps prefabs already assigned
    void SyncModels(SerializedProperty list, List<string> names)
    {
        var old = new Dictionary<string, UnityEngine.Object>();
        for (int i = 0; i < list.arraySize; i++)
        {
            var el = list.GetArrayElementAtIndex(i);
            old[el.FindPropertyRelative("treeName").stringValue] =
                el.FindPropertyRelative("prefab").objectReferenceValue;
        }

        list.arraySize = names.Count;
        for (int i = 0; i < names.Count; i++)
        {
            var el = list.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("treeName").stringValue = names[i];
            old.TryGetValue(names[i], out var prefab);
            el.FindPropertyRelative("prefab").objectReferenceValue = prefab;
        }
        serializedObject.ApplyModifiedProperties();
    }

    bool IsPrefabContext()
    {
        var go = Script.gameObject;
        return PrefabUtility.IsPartOfPrefabAsset(go)
               || PrefabStageUtility.GetPrefabStage(go) != null;
    }

    static TextField MakeReadOnlyField(string label)
    {
        var field = new TextField(label) { isReadOnly = true };
        field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
        return field;
    }

    static VisualElement MakeDivider()
    {
        var d = new VisualElement();
        d.style.height = 1;
        d.style.marginTop = 6;
        d.style.marginBottom = 6;
        d.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        return d;
    }
}