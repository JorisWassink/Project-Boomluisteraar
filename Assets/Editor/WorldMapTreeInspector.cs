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

    List<string> _ids;

    TreeWorldMap Script => (TreeWorldMap)target;
    Transform ModelParent => Script.transform.Find(ModelParentName);

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        _ids = StaticVariables.GetTreeIds;
        var choices = _ids.Select(StaticVariables.GetTreeName).ToList();

        var idProp = serializedObject.FindProperty("id");
        var modelsProp = serializedObject.FindProperty("models");

        int startIndex = Mathf.Max(0, _ids.IndexOf(idProp.stringValue));

        var dropdown = new DropdownField("Tree", choices, startIndex);
        dropdown.SetEnabled(!IsPrefabContext());
        dropdown.RegisterValueChangedCallback(_ => OnTreeSelected(_ids[dropdown.index]));

        _idField = MakeReadOnlyField("ID");
        _sensorTypeField = MakeReadOnlyField("Sensor Type");

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(MakeDivider());
        root.Add(dropdown);
        root.Add(_idField);
        root.Add(_sensorTypeField);
        root.Add(MakeDivider());

        root.TrackPropertyValue(modelsProp, _ =>
        {
            if (!IsPrefabContext()) SpawnModel(_ids[Mathf.Max(0, dropdown.index)]);
        });

        UpdateInfoFields(_ids[startIndex]);

        if (string.IsNullOrEmpty(idProp.stringValue) && !IsPrefabContext())
            OnTreeSelected(_ids[startIndex]);

        return root;
    }

    void UpdateInfoFields(string id)
    {
        var entry = StaticVariables.GetTreeEntry(id);
        _idField.SetValueWithoutNotify(id);
        _sensorTypeField.SetValueWithoutNotify(entry.type.ToString());
    }

    void OnTreeSelected(string id)
    {
        UpdateInfoFields(id);

        if (IsPrefabContext()) return;

        var treeName = StaticVariables.GetTreeEntry(id).name;

        serializedObject.Update();
        serializedObject.FindProperty("id").stringValue = id;
        serializedObject.ApplyModifiedProperties();

        var go = Script.gameObject;
        if (go.name != treeName)
        {
            Undo.RecordObject(go, "Rename tree");
            go.name = treeName;
        }

        SpawnModel(id);
    }

    void SpawnModel(string id)
    {
        var parent = ModelParent;
        if (parent == null)
        {
            Debug.LogWarning($"'{Script.name}' has no child called '{ModelParentName}'.", Script);
            return;
        }

        var prefab = FindPrefab(id);
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

    GameObject FindPrefab(string id)
    {
        return StaticVariables.GetTreeModel(id);
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