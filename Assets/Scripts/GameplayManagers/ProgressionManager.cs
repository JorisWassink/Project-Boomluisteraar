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
    [HideInInspector] public bool IsFirstStart = true;
    
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

        int highest = -1;
        int lowest = 999;
        
        foreach ((string key, int requiredExp) in expRequirements)
        {
            if (playerExperience >= requiredExp && requiredExp > highest)
            {
                currentGuideTreeId = key;
                highest = requiredExp;
                continue;
            }

            if (requiredExp < lowest && requiredExp > playerExperience)
            {
                nextExperienceTarget = requiredExp;
                lowest = requiredExp;
            }
        }
        
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

    public void UnlockCollectible(string collectibleID)
    {
        collectibles[collectibleID] = true;
    }

    public bool CollectibleIsUnlocked(string collectibleID)
    {
        return collectibles[collectibleID];
    }

    public bool treeIsUnlocked(string treeID)
    {
        return unLockedTrees.Contains(treeID);
    }
    
    //Private functions
    private void UpdatePoints(int points)
    {
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
            }
        }
        
        foreach (var tree in unLockedTrees)
            expRequirements.Remove(tree);
    }
}
