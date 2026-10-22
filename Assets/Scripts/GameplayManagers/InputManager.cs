using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    //TODO: make this use managers
    [SerializeField] private Camera currentCamera;
    
    [Tooltip("How long before the click becomes a drag")]
    [SerializeField] public float clickDuration;
    
    public UnityEvent<Vector2> onMousePressed;
    public UnityEvent<Vector2> onMouseHold;
    public UnityEvent<Vector2> onMouseRelease;
    public UnityEvent<Vector2> onMouseClick;
    
    public static InputManager Instance;

    private Vector2 _mousePositionAtClick = Vector2.zero; 
    
    private static Vector2 PointerPos =>
        Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
  
    //Unity event functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    //Own functions
    public void InvokeMouseClick(InputAction.CallbackContext context)
    {
        
        Vector2 clickPosition = PointerPos;
        Vector3 rayPos = GetMousePositionInWorld(clickPosition);
        Debug.Log(context.interaction);
        Debug.DrawRay(rayPos, Vector3.up * 50, Color.red, 999);
        
        if (context.performed) onMouseHold.Invoke(clickPosition);
        if (context.started)
        {
            _mousePositionAtClick = Mouse.current.position.ReadValue();
            onMousePressed.Invoke(clickPosition);
        }
        
        if (context.canceled)
        {
            onMouseRelease.Invoke(clickPosition);
            if (context.duration > clickDuration) return;
            if (Mouse.current.position.ReadValue() == _mousePositionAtClick)
                onMouseClick.Invoke(clickPosition);
        }
    }
    
    //Helpers
    public Vector3 GetMousePositionInWorld(Vector2 clickPosition)
    {
        Vector3 cameraPosition = currentCamera.transform.position;
        
        Vector3 mouseVec3 = new Vector3(clickPosition.x, clickPosition.y, 1f); 
        Vector3 mouseWorldPosition = currentCamera.ScreenToWorldPoint(mouseVec3);
        
        RaycastHit raycastHit;
        if (Physics.Raycast(cameraPosition, mouseWorldPosition - cameraPosition,
                out raycastHit))
        {
            return raycastHit.point;
        }
        
        //if no collider has been found, default to y = 0
        float floorY = 0;
        float maxY = mouseWorldPosition.y;
        float minY = cameraPosition.y;
        
        float a = (1-0)/(maxY-minY);
        
        float b = 0 - (a * minY);
        
        float t = floorY * a + b;
        
        Vector3 pointOnFloor = Vector3.Lerp(cameraPosition, mouseWorldPosition, t);
        return pointOnFloor;
    }
 
}

