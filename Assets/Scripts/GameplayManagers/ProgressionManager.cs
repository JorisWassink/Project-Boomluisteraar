using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ProgressionManager : MonoBehaviour
{
    [HideInInspector] public int playerExperience = 0;
    [HideInInspector] public UnityEvent onPointsUpdate = new UnityEvent();
    
    public static ProgressionManager Instance;

    //Unity event functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    //Public functions
    public void AddPoints(int points)
    {
        int newPoints = playerExperience + points;
        UpdatePoints(newPoints);
    }

    public void RemovePoints(int points)
    {
        playerExperience -= points;
    }
    
    //Private functions
    private void UpdatePoints(int points)
    {
        playerExperience = points;
        onPointsUpdate.Invoke();
    }
}
