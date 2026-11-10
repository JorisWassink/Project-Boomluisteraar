using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    [Tooltip("Temporary fix, just put the camera here")]
    [SerializeField] private Camera currentCamera;
    
    [Tooltip("How long before the click becomes a drag")]
    [SerializeField] public float clickDuration;
    
    [NonSerialized] public UnityEvent<Vector2> onMousePressed = new UnityEvent<Vector2>();
    [NonSerialized] public UnityEvent<Vector2> onMouseHold= new UnityEvent<Vector2>();
    [NonSerialized] public UnityEvent<Vector2> onMouseRelease= new UnityEvent<Vector2>();
    [NonSerialized] public UnityEvent<Vector2> onMouseClick= new UnityEvent<Vector2>();
    
    private TreeSelectionManager _treeSelectionManager;
    private DeviceChecker _deviceChecker;
    
    private Vector2 _mousePositionAtClick;
    private bool _mouseActive = true;
    
    private static Vector2 PointerPos =>
        Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
    public static InputManager Instance;
  
    //Unity Event Functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _deviceChecker = gameObject.AddComponent<DeviceChecker>();
        _treeSelectionManager = TreeSelectionManager.Instance;
        
        _mousePositionAtClick = Vector2.zero;
        //_deviceChecker.LogDeviceData();
        if(_treeSelectionManager != null)
            _treeSelectionManager.onSetUIMode.AddListener(SetUIMode);
    }
    
    //Public Functions
    public void InvokeMouseClick(InputAction.CallbackContext context)
    {
        if(!_mouseActive) return;
        
        Vector2 clickPosition = PointerPos;
        Vector3 rayPos = GetMousePositionInWorld(clickPosition);
        Debug.DrawRay(rayPos, Vector3.up * 50, Color.red, 5);
        
        if (context.performed) onMouseHold.Invoke(clickPosition);
        if (context.started)
        {
            _mousePositionAtClick = PointerPos;
            onMousePressed.Invoke(clickPosition);
        }
        
        if (context.canceled)
        {
            onMouseRelease.Invoke(clickPosition);
            if (context.duration > clickDuration) return;
            if (PointerPos == _mousePositionAtClick)
                onMouseClick.Invoke(clickPosition);
        }
    }

    //Private Functions
    private void SetUIMode(bool value)
    {
        _mouseActive = !value;
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

