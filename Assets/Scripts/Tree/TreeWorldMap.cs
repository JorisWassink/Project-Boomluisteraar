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
    [SerializeField] private RawImage lockSprite;
    
    [HideInInspector] public string id;
    [HideInInspector] public List<TreeModel> models = new();
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
        InputManager.Instance.onMouseClick.AddListener(OnMouseClicked);
    }

    private void Update()
    {
        if(_cam == null || _isUnlocked) return;
        lockSprite.transform.rotation.SetLookRotation(_cam.transform.position);
    }

    //Public functions
    public void SetLockedStatus(bool isLocked)
    {
        _isUnlocked = !isLocked;
        lockSprite.enabled = isLocked;

        MeshRenderer[] mr = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in mr)
        {
            renderer.material.color =  isLocked ? Color.gray2 : Color.red;
        }
    }
    
    //private functions
    private void OnMouseClicked(Vector2 clickPosition)
    {
        if(!_isUnlocked) return;
        
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld(clickPosition);
        
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
