using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TreeSpawner : MonoBehaviour
{
    //temp script for menu
    [SerializeField] private GameObject treePrefab;
    List<TreeWorldMap> _trees  = new List<TreeWorldMap>();
    
    private List<string> AllSensorIds = new List<string>()
    {
        "9B261005",
        "91261120",
        "9B261002",
        "91261115",
        "91261114",
        "9B261009",
        "91261121"
    };

    private void Start()
    {
        Vector3 offset =  new Vector3(0f, 0f, 30f);
        float radiansPerTree = (2 * Mathf.PI) / AllSensorIds.Count;

        for (int treeIndex = 0; treeIndex < AllSensorIds.Count; treeIndex++)
        {
            float s = Mathf.Sin(radiansPerTree * treeIndex);
            float c = Mathf.Cos(radiansPerTree * treeIndex);
            
            float treeX = offset.x * c - offset.z * s;
            float treeZ = offset.x * s + offset.z * c;
            
            Vector3 treePosition = new Vector3(treeX, 0f, treeZ);
            GameObject treeInstanceObject = Instantiate(treePrefab, treePosition, Quaternion.identity, transform);
            TreeWorldMap treeInstance = treeInstanceObject.GetComponent<TreeWorldMap>();
            treeInstance.InitializeTree(AllSensorIds[treeIndex]);
            _trees.Add(treeInstance);
        }
    }

    public void OnTap(Vector2 pos)
    {
        Debug.DrawLine(transform.position, pos, Color.red);
    }
    
    
}
