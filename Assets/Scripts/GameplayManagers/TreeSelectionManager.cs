using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TreeSelectionManager : MonoBehaviour
{
    [HideInInspector] public UnityEvent<bool> onSetUIMode = new UnityEvent<bool>();
    [HideInInspector] public static TreeSelectionManager Instance;
    
    //Unity Event Functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    //Public Functions
    public void SetUIMode(bool value)
    {
        Debug.Log("SetUIMode: " + value);
        onSetUIMode.Invoke(value);
    }
}
