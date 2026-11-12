using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    [Tooltip("How long before the click becomes a drag")]
    [SerializeField] public float clickDuration;
    
    [NonSerialized] public UnityEvent<Vector3> onPointInputStart = new ();
    [NonSerialized] public UnityEvent<Vector3> onPointInputEnd= new ();
    [NonSerialized] public UnityEvent<Vector3> onPointInputTap= new ();
    
    private GameManager _gameManager;
    private DeviceChecker _deviceChecker;
    
    private Vector2 _mousePositionAtClick;
    private bool _canPoint = true;
    
    private Camera CurrentCamera => _gameManager.currentCamera;
    
    private static Vector2 PointerPos =>
        Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
    
    public static InputManager Instance;
  
    //Unity Event Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        
        _deviceChecker = gameObject.AddComponent<DeviceChecker>();
        
        _mousePositionAtClick = Vector2.zero;
        //_deviceChecker.LogDeviceData();
    }
    
    //Public Functions
    public void OnPointInput(InputAction.CallbackContext context)
    {
        if(!_canPoint) return;
        
        Vector3 worldPos = GetMousePositionInWorld(PointerPos);
        
        if (context.started)
        {
            _mousePositionAtClick = PointerPos;
            onPointInputStart.Invoke(worldPos);
        }
        
        if (context.canceled)
        {
            onPointInputEnd.Invoke(worldPos);
            if (context.duration > clickDuration) return;
            if (PointerPos == _mousePositionAtClick)
                onPointInputTap.Invoke(worldPos);
        }
        
        Debug.DrawRay(worldPos, Vector3.up * 50, Color.red, 5);
    }

    public void SetUIMode(bool value)
    {
        _canPoint = !value;
    }
    
    //Helpers
    public Vector3 GetMousePositionInWorld(Vector2 clickPosition)
    {
        Vector3 cameraPosition = CurrentCamera.transform.position;
        
        Vector3 mouseVec3 = new Vector3(clickPosition.x, clickPosition.y, 1f); 
        Vector3 mouseWorldPosition = CurrentCamera.ScreenToWorldPoint(mouseVec3);
        
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

