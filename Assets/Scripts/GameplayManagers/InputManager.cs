using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    //TODO: make this use managers
    [SerializeField] private Camera currentCamera;
    
    public static InputManager Instance;
  

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public Vector3 GetMousePositionInWorld()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mouseVec3 = new Vector3(mousePosition.x, mousePosition.y, mousePosition.y);
        Vector3 mouseWorldPosition = currentCamera.ScreenToWorldPoint(mouseVec3);
        
        RaycastHit raycastHit;
        if (Physics.Raycast(currentCamera.transform.position, mouseWorldPosition - currentCamera.transform.position,
                out raycastHit))
        {
            return raycastHit.point;
        }
        
        //if no collider has been found, default to y = 0
        float floorY = 0;
        float maxY = mouseWorldPosition.y;
        float minY = currentCamera.transform.position.y;
        
        float a = (1-0)/(maxY-minY);
        
        float b = 0 - (a * minY);
        
        float t = floorY * a + b;
        
        Vector3 pointOnFloor = Vector3.Lerp(currentCamera.transform.position, mouseWorldPosition, t);
        return pointOnFloor;
    }

    private void OnDrawGizmos()
    {
        Vector3 mousePositionInWorld = GetMousePositionInWorld();
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(mousePositionInWorld, 1f);
    }
}

