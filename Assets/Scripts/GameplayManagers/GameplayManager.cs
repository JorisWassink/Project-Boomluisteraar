using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] public Camera currentCamera;
    public static GameplayManager Instance;
    
    public TreeGameplay currentTreeGameplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
}
