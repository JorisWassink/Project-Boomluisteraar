using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public int PlayerExperience = 0;
    public static ProgressionManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
