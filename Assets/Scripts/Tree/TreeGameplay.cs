using System;
using UnityEngine;

public class TreeGameplay : TreeInstance
{
    [SerializeField] private GameObject modelParent;
    
    //Public functions
    public override void InitializeTree(string id)
    {
        GameObject modelObject = StaticVariables.GetTreeModel(id);
        Instantiate(modelObject, modelParent.transform);
        base.InitializeTree(id);
    }
}
