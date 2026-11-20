using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRegistrar : MonoBehaviour
{
    private void Awake() => GameManager.Instance.SetCurrentCamera(GetComponent<Camera>());
    void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentCamera == GetComponent<Camera>())
            GameManager.Instance.currentCamera = null;
    }
}