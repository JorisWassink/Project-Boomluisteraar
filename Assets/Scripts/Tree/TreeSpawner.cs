using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TreeSpawner : MonoBehaviour
{
    //temp script for menu
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private Camera cam;
    List<TreeWorldMap> _trees  = new List<TreeWorldMap>();
    public static UnityEvent OnTreeTapped = new UnityEvent();
    
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

    public void OnTap(InputAction.CallbackContext context)
    {
        //todo: not hardcode this
        float floorY = 0;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseVec3 = new Vector3(mousePosition.x, mousePosition.y, mousePosition.y);
        Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseVec3);
        

        float maxY = mouseWorldPosition.y;
        float minY = cam.transform.position.y;
        
        float a = (1-0)/(maxY-minY);
        
        float b = 0- (a * minY);
        
        float t = floorY * a + b;
        
        Vector3 pointOnFloor = Vector3.Lerp(cam.transform.position, mouseWorldPosition, t);
        
        
        Debug.DrawLine(cam.transform.position, pointOnFloor,  Color.red, 99999);
        
        
        foreach (var tree in _trees)
        {
            float distance =  Vector3.Distance(pointOnFloor, tree.transform.position);
            if (distance < 10)
            {
                StaticVariables.SelectedTreeId = tree.treeId;
                SceneManager.LoadScene("GameplayScene");
            }
        }
        
    }
}
