using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TreeWorldMap : TreeInstance
{
    [SerializeField] private string id = "9B261005";
    [SerializeField] private float hitboxRadius = 10f;
    
    private void Awake()
    {
        InitializeTree(id);
    }

    private void Start()
    {
        InputManager.Instance.onMouseClick.AddListener(OnMouseClicked);
    }

    private void OnMouseClicked()
    {
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        Vector3 treePos = transform.position;
        treePos.y = 0;
        pointOnFloor.y = 0;
        
        Debug.Log($"Point On Floor: {pointOnFloor}, treePos: {treePos}");
        float distance =  Vector3.Distance(pointOnFloor, transform.position);
        if (distance < hitboxRadius)
        {
            StaticVariables.SelectedTreeId = treeId;
            SceneManager.LoadScene("GameplayScene");
        }
    }

    //Debug Functions
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitboxRadius);
    }
}
