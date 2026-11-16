using System;
using UnityEngine;

public abstract class TreeInstance : MonoBehaviour
{
    [HideInInspector] public string treeId; //the sensor id, only for the tree types
    [HideInInspector] public string treeNameLatin;
    [HideInInspector] public string treeNameDutch;
    
    public virtual void InitializeTree(string id)
    {
        treeId = id;
        treeNameLatin = StaticVariables.GetTreeName(id, false);
        treeNameDutch = StaticVariables.GetTreeName(id, true);
        gameObject.name = treeNameLatin;
    }
}
