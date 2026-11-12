using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ProgressionManager : MonoBehaviour
{
    [HideInInspector] public int playerExperience = 0;
    [HideInInspector] public UnityEvent onPointsUpdate = new UnityEvent();
    
    private Dictionary<string, bool> collectibles = new Dictionary<string, bool>();
    

    //Unity event functions
    private void Start()
    {
        List<string> allCollectibleIds = StaticVariables.GetAllCollectibleIds;
        foreach (string collectibleId in allCollectibleIds)
        {
            collectibles.Add(collectibleId, false);
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

    public bool CollectibleIsUnlocked(string collectibleID)
    {
        return collectibles[collectibleID];
    }
    
    //Private functions
    private void UpdatePoints(int points)
    {
        playerExperience = points;
        onPointsUpdate.Invoke();
    }
    
    //Debug Functions
    public void PrintAllCollectibleNamesDebug()
    {
        string message = $"Printing all collectible names\n";

        message = collectibles.Aggregate(message, (current, collectible) => current + $"{StaticVariables.GetCollectibleName(collectible.Key)}\n");

        Debug.Log(message);
    }
}
