using UnityEngine;
using UnityEngine.InputSystem;

public class WorldMapCamera : MonoBehaviour
{
    Vector3 dragOrigin;
    
    Vector3 lastFramePosition =  Vector3.zero;
    Vector3 dragAmount;
    private float dragSpeed = 1;
    
    private bool dragging = false;
    
    public void OnDrag(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            dragOrigin = InputManager.Instance.GetMousePositionInWorld();
            dragging = true;
        }

        if (context.canceled)
        {
            dragging = false;
        }
        
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        
    }

    private void Update()
    {
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        Vector3 dragDifference = pointOnFloor - dragOrigin;
        
        if (dragging)
        {
            Vector3 move = new Vector3(dragDifference.x * -dragSpeed, 0, dragDifference.y * -dragSpeed);
            
            
            transform.Translate(move, Space.World);
            
            
            lastFramePosition = pointOnFloor;
        }
    }
}
