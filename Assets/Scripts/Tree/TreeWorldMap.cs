using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeWorldMap : TreeInstance
{
    [SerializeField] public int requiredExperience = 0;
    
    [SerializeField] private float hitboxRadius = 10f;
    [SerializeField] private float treeHeight = 20f;    
    
    [SerializeField] private RawImage lockSprite;
    
    [HideInInspector] public string id;
    [HideInInspector] public List<TreeModel> models = new();
    [HideInInspector] public Vector3 modelRotation;
    
    [HideInInspector] public Vector3 pointPosition;
    
    //TODO: fix camera getting
    private Camera _cam;
    private bool _isUnlocked = true;
    
    //Unity event functions
    private void Awake()
    {
        InitializeTree(id);
    }

    private void Start()
    {
        pointPosition = GetModelCenterOnFloor();
    }

    private void Update()
    {
        if(_cam == null || _isUnlocked) return;
        lockSprite.transform.rotation.SetLookRotation(_cam.transform.position);
    }

    //Public functions
    public void SetLockedStatus(bool isLocked)
    {
        var vector3 = lockSprite.gameObject.transform.position;
        vector3.y = treeHeight + 5;
        lockSprite.gameObject.transform.position = vector3;
        
        _isUnlocked = !isLocked;
        lockSprite.enabled = isLocked;

        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in mr)
        {
            renderer.material.color =  isLocked ? Color.gray2 : Color.red;
        }
    }
    
    public void TrySelectTree(float distance)
    {
        if(!_isUnlocked) return;
        
        Debug.Log($"Distance: {distance}, hitboxRadius: {hitboxRadius}");
        if (distance < hitboxRadius)
        {
            StaticVariables.SelectedTreeId = treeId;
            GameManager.Instance.SwitchScene(Scene.TreeView);
        }
    }
    
    //Private Functions
    private Vector3 GetModelCenterOnFloor()
    {
        float cornerAngle = Mathf.Sin(90);
        float modelAngle = Mathf.Sin(modelRotation.x);

        float a = (treeHeight / modelAngle) * cornerAngle;
        
        Vector3 floorLine = new(0, 0, 1);
        Vector3 treeLine = Quaternion.AngleAxis(modelRotation.x, Vector3.right) * Vector3.up;
        
        float dot = Vector3.Dot(treeLine, floorLine) * a;
        
        var halfWay = new Vector3(transform.position.x, transform.position.y, transform.position.z + dot / 2);
        return halfWay;
    }

    //Debug Functions
    private void OnDrawGizmos()
    {
        Vector3 center = GetModelCenterOnFloor();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, hitboxRadius);
        Vector3 position = pointPosition + new Vector3(0, treeHeight, 0);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(position , new Vector3(hitboxRadius, 0, hitboxRadius));
    }
}
