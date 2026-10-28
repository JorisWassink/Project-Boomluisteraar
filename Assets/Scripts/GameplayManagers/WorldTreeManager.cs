using System.Collections.Generic;
using UnityEngine;

public class WorldTreeManager : MonoBehaviour
{
    //TODO: editor tooling maybe?
    private List<TreeWorldMap> _trees = new List<TreeWorldMap>();
    private ProgressionManager progressionManager;
    
    public static WorldTreeManager Instance;
    
    //Unity Event Functions
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        _trees = new List<TreeWorldMap>(GetComponentsInChildren<TreeWorldMap>());
        progressionManager = ProgressionManager.Instance;
        
        //TODO: make this be called when the worldmap gets loaded, for now this works i think
        CheckProgression();
    }
    
    //Own functions
    private void CheckProgression()
    {
        int playerExperience = progressionManager.PlayerExperience;
        foreach (TreeWorldMap tree in _trees)
        {
            bool shouldBeLocked = playerExperience < tree.requiredExperience;
            tree.SetLockedStatus(shouldBeLocked);
        }
    }
}
