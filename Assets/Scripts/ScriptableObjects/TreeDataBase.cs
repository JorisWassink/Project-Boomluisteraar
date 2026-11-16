using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeDataBase", menuName = "Scriptable Objects/TreeDataBase")]
public class TreeDataBase : ScriptableObject
{
    [Serializable]
    public class TreeEntry
    {
        public string name;
        public string dutchName;
        public string id;
        public int requiredExp;
        public StaticVariables.DataType type;
        public GameObject model;
    }
    
    [Serializable]
    public class SoilEntry
    {
        public string name;
        public string id;
        public StaticVariables.DataType type;
    }

    public List<TreeEntry> trees = new();
    public List<SoilEntry> soils = new();

    public GameObject GetTreeModel(string id)
    {
        return GetTreeEntry(id).model;
    }
    
    public string GetTreeName(string id, bool dutch)
    {
        return dutch ? GetTreeEntry(id).dutchName : GetTreeEntry(id).name;
    }
    
    public int GetTreeExpRequirement(string id)
    {
        return GetTreeEntry(id).requiredExp;
    }

    public StaticVariables.DataType GetTreeType(string id)
    {
        return GetTreeEntry(id).type;
    }

    public List<string> GetTreeIds()
    {
        List<string> ids = trees.ConvertAll(t => t.id);
        return ids;
    }

    public TreeEntry GetTreeEntry(string id)
    {
        foreach (var t in trees)
            if (t.id == id) return t;
        return null;
    }
    
    public TreeEntry GetTreeEntryByName(string name)
    {
        foreach (var t in trees)
            if (t.name == name) return t;
        return null;
    }
}