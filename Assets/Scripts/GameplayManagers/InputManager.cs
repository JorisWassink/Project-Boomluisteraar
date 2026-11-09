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

    //QUICKFIX MOMENT
    [SerializeField] private float tapMovementTolerance = 40f;

    private Vector2 _pressPosition;
    private double _pressTime;
    private bool _pressActive;

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
        _deviceChecker.LogDeviceData();
        
        _treeSelectionManager.onSetUIMode.AddListener(SetUIMode);
    }

    //Public Functions


    //Maybe change the name of this func to somethjig like HandlePointerInput? maybe its just my ocd XD
    public void InvokeMouseClick(InputAction.CallbackContext context)
    {
        if (context.control.device is not Pointer pointer)
            return;

        Vector2 position = pointer.position.ReadValue();

        if (context.started)
        {
            _pressPosition = position;
            _pressTime = context.time;
            _pressActive = _mouseActive;

            if (_pressActive)onMousePressed.Invoke(position);
        }

        if (context.performed && _pressActive)
        {
            onMouseHold.Invoke(position);
        }

        if (context.canceled)
        {
            if (!_pressActive) return;

            _pressActive = false;

            onMouseRelease.Invoke(position);

            if (!_mouseActive) return;

            double duration = context.time - _pressTime;
            float movement = Vector2.Distance(_pressPosition, position);

            float maxDuration = clickDuration > 0f ? clickDuration : 0.4f;

            bool validTap = duration <= maxDuration && movement <= tapMovementTolerance;

            Debug.Log($"Tap detected: duration={duration:F2}, " + $"movement={movement:F1}, valid={validTap}");

            if (validTap)
            {
                onMouseClick.Invoke(position);
            }
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
        if (Physics.Raycast(cameraPosition, mouseWorldPosition - cameraPosition, out raycastHit))
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

    public Ray GetPointerRay(Vector2 screenPosition)
    {
        return currentCamera.ScreenPointToRay(screenPosition);
    }

}

