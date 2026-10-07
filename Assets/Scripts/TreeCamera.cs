 using System;
 using Unity.VisualScripting;
 using UnityEngine;

public class TreeCamera : MonoBehaviour
{
    [SerializeField] private GameObject treeParent;
    [SerializeField] private Vector3 offset;

    private int treeIndex = 0;
    void Start()
    {
        OnTreeSelected();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (treeIndex > 0)
                treeIndex--;
            else
                treeIndex = treeParent.transform.childCount - 1;
            OnTreeSelected();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(treeIndex < treeParent.transform.childCount - 1)
                treeIndex++;
            else
                treeIndex = 0;
            OnTreeSelected();
        }

        
    }

    private void OnTreeSelected()
    {
        Transform tree = treeParent.transform.GetChild(treeIndex);
        transform.rotation = Quaternion.LookRotation(tree.transform.position - transform.position);
    }
    
}
