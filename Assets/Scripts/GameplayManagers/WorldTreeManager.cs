using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldTreeManager : MonoBehaviour
{
    private List<TreeWorldMap> _trees = new ();
    
    private GameManager _gameManager;
    private ProgressionManager ProgressionManager => _gameManager.progressionManager;
    private InputManager InputManager => _gameManager.inputManager;

    
    void Start()
    {
        _trees = new List<TreeWorldMap>(GetComponentsInChildren<TreeWorldMap>());
        
        _gameManager = GameManager.Instance;
        
        InputManager.onPointInputTap.AddListener(OnPointInput);
        
        CheckProgression();
    }

    private void OnDestroy()
    {
        InputManager.onPointInputTap.RemoveListener(OnPointInput);
    }

    //private functions
    private void OnPointInput(Vector3 pointPosition)
    {
        int closestTreeIndex = -1;
        float closestDistance = float.MaxValue;

        for (int index = 0; index < _trees.Count; index++)
        {
            var tree = _trees[index];
            float currentDistance = Vector3.Distance(pointPosition, tree.transform.position);
            if (currentDistance <= closestDistance)
            {
                closestDistance = currentDistance;
                closestTreeIndex = index;
            }
        }
        
        //test for hitbox in tree so they can be independent 
        _trees[closestTreeIndex].TrySelectTree(closestDistance);
    }
    
    private void CheckProgression()
    {
        int playerExperience = ProgressionManager.playerExperience;
        foreach (TreeWorldMap tree in _trees)
        {
            bool shouldBeLocked = playerExperience < tree.requiredExperience;
            tree.SetLockedStatus(shouldBeLocked);
        }
    }
}
