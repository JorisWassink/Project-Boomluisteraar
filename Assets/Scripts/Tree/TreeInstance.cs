using System;
using UnityEngine;

public abstract class TreeInstance : MonoBehaviour
{
    [HideInInspector] public string treeId; //the sensor id, only for the tree types
    [HideInInspector] public string treeName;
    
    public virtual void InitializeTree(string id)
    {
        treeId = id;
        treeName = StaticVariables.TryGetTreeData(id).Item1;
        gameObject.name = treeName;
    }
}
