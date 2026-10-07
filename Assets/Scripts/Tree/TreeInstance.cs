using System;
using UnityEngine;

public class TreeInstance : MonoBehaviour
{
    [HideInInspector] public string treeId; //the sensor id, only for the tree types
    [HideInInspector] public string treeName;

    private void Awake()
    {
        //for now call in here
        InitializeTree("9B261005");
    }
    
    public void InitializeTree(string id)
    {
        treeId = id;
        treeName = StaticVariables.TryGetTreeData(id).Item1;
        gameObject.name = treeName;
    }
}
