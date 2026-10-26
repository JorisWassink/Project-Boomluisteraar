using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(TreeWorldMap))]
public class WorldMapTree_Inspector : Editor
{
    TextField _idField;
    TextField _sensorTypeField;

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        var choices = StaticVariables.TreeIdDictionary.Values
            .Where(v => v.Item2 != StaticVariables.DataType.SoilSensor)
            .Select(v => v.Item1)
            .ToList();

        var dropdown = new DropdownField("Tree", choices, 0);
        dropdown.BindProperty(serializedObject.FindProperty("treeId"));

        _idField = new TextField("ID") { isReadOnly = true };
        _idField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
        
        _sensorTypeField = new TextField("Sensor Type") { isReadOnly = true };
        _sensorTypeField.AddToClassList(BaseField<StaticVariables.DataType>.alignedFieldUssClassName);

        dropdown.RegisterValueChangedCallback(evt => OnTreeSelected(evt.newValue));

        root.Add(dropdown);
        root.Add(_idField);
        root.Add(_sensorTypeField);

        var divider = new VisualElement();
        divider.style.height = 1;
        divider.style.marginTop = 6;
        divider.style.marginBottom = 6;
        divider.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        root.Add(divider);

        // normal inspector
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        OnTreeSelected(dropdown.value);
        return root;
    }

    void OnTreeSelected(string treeName)
    {
        var entry = StaticVariables.TreeIdDictionary
            .FirstOrDefault(kv => kv.Value.Item1 == treeName);

        _idField.SetValueWithoutNotify(entry.Key);
        _sensorTypeField.SetValueWithoutNotify(entry.Value.Item2.ToString());
        var idProp = serializedObject.FindProperty("id");
        idProp.stringValue = entry.Key;
        serializedObject.ApplyModifiedProperties();
    }
}
