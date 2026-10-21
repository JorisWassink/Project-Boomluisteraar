using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TreeWorldMap : TreeInstance
{
    [SerializeField] private string id = "9B261005";
    [SerializeField] private float hitboxRadius = 10f;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private RawImage lockSprite;
    
    private void Awake()
    {
        InitializeTree(id);
        if (!isUnlocked) SetLocked();
    }

    private void Start()
    {
        InputManager.Instance.onMouseClick.AddListener(OnMouseClicked);
    }

    //Own functions
    private void OnMouseClicked()
    {
        if(!isUnlocked) return;
        
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        
        float distance =  Vector3.Distance(pointOnFloor, transform.position);
        if (distance < hitboxRadius)
        {
            StaticVariables.SelectedTreeId = treeId;
            SceneManager.LoadScene("GameplayScene");
        }
    }

    private void SetLocked()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<MeshRenderer>(out var component))
            {
                component.material.color = Color.gray2;
            }
        }

        lockSprite.enabled = true;
        lockSprite.transform.rotation.SetLookRotation(transform.up);
    }

    //Debug Functions
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitboxRadius);
    }
}
