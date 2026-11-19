using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class ProgressionManager : MonoBehaviour
{
    [HideInInspector] public int playerExperience = 0;
    
    [HideInInspector] public int nextExperienceTarget = 0;
    
    [HideInInspector] public UnityEvent onPointsUpdate = new UnityEvent();
    [HideInInspector] public string currentGuideTreeId {get; private set;} 
    
    private Dictionary<string, bool> collectibles = new Dictionary<string, bool>();
    private List<string> unLockedTrees = new List<string>();
    private Dictionary<string, int> expRequirements = new ();
    
    //Unity event functions
    private void Awake()
    {
        List<string> treeIds = StaticVariables.GetTreeIds;
        foreach (string id in treeIds)
        {
            expRequirements.Add(id, StaticVariables.GetTreeExpRequirement(id));
        }

        List<int> requirements = expRequirements.Values.ToList();
        requirements.Sort();
        nextExperienceTarget = requirements[0];
        
        UpdatePoints(0);
    }

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
        if (points >= nextExperienceTarget)
        {
            // unLockedTrees.Add(currentTargetTree);
        }
        playerExperience = points;
        SetUnlockedTrees();
        onPointsUpdate.Invoke();
    }
    
    private void SetUnlockedTrees()
    {
        foreach (var tree in expRequirements)
        {
            if (playerExperience >= tree.Value)
            {
                unLockedTrees.Add(tree.Key);
                currentGuideTreeId = tree.Key;
                Debug.Log("Unlocked " + tree.Key);
            }
        }
        
        foreach (var tree in unLockedTrees)
            expRequirements.Remove(tree);
    }
}
