using UnityEngine;
using UnityEngine.InputSystem;

public class WorldMapCamera : MonoBehaviour
{
    public Rect bounds;
    public Vector2 viewportSize;
    
    private Vector3 _dragOrigin;
    private Vector3 _dragAmount;
    private Vector3 _cancelPosition;
    private float _dragSpeed = 1;
    private Camera _camera;

    private Vector3 _startCamPos;
    
    private bool _dragging = false;
    private bool _positionDirty = false;
    private float _cancelTime;

    //event functions
    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        Vector3 dragDifference = pointOnFloor - _dragOrigin;
        
        if (_dragging)
        {
            Vector3 diff = _dragOrigin - GroundPointFrom(_startCamPos);
            diff.y = 0;
            transform.position = _startCamPos + diff * _dragSpeed;
        } else if (_positionDirty)
        {
            Vector3 p = transform.position;
            Vector3 look = LookPoint();
            Vector3 clamped = look;
            clamped.x = Mathf.Clamp(look.x, bounds.xMin + viewportSize.x / 2, bounds.xMax - viewportSize.x / 2);
            clamped.z = Mathf.Clamp(look.z, bounds.yMin + viewportSize.y / 2, bounds.yMax - viewportSize.y / 2);
            Vector3 target = p + (clamped - look);

            transform.position = Vector3.Lerp(p, target, 10f * Time.deltaTime);

            if ((transform.position - target).sqrMagnitude < 0.0001f)
                _positionDirty = false;
        }
    }
    
    //own functions
    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startCamPos = transform.position;
            _dragOrigin = GroundPointFrom(_startCamPos);
            _dragging = true;
        }

        if (context.canceled)
        {
            _cancelTime = Time.deltaTime;
            _dragging = false;
            _positionDirty = true;
            _cancelPosition = Physics.Raycast(transform.position, Vector3.down ,out var raycastHit) ? 
                raycastHit.point : new Vector3(transform.position.x, 0, transform.position.z);
        }
    }
    
    //helpers
    private Vector3 GroundPointFrom(Vector3 camPos)
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        return camPos + ray.direction * (-camPos.y / ray.direction.y);
    }
    
    private Vector3 LookPoint()
    {
        Vector3 p = transform.position;
        return p + transform.forward * (-p.y / transform.forward.y);
    }
}
