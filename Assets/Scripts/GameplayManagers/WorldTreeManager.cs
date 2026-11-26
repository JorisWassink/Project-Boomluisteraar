using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldTreeManager : MonoBehaviour
{
    private List<TreeWorldMap> _trees = new ();
    
    private GameManager _gameManager;
    private ProgressionManager _progressionManager;
    private InputManager _inputManager;

    //Unity Event Functions
    private void Start()
    {
        _trees = new List<TreeWorldMap>(GetComponentsInChildren<TreeWorldMap>());
        
        _gameManager = GameManager.Instance;
        _inputManager = _gameManager.inputManager;
        _progressionManager = _gameManager.progressionManager;
        
        CheckProgression();

        if (InputManager.Instance == null)
        {
            Debug.LogError("WorldTreeManager: InputManager missing!");
            return;
        }
        _inputManager.onPointInputTap.AddListener(OnPointInput);
    }

    private void OnDestroy()
    {
        _inputManager.onPointInputTap.RemoveListener(OnPointInput);
    }

    //private functions
    private void OnPointInput(Vector3 pointPosition)
    {
        int closestTreeIndex = -1;
        float closestDistance = float.MaxValue;

        for (int index = 0; index < _trees.Count; index++)
        {
            var tree = _trees[index];
            float currentDistance = Vector3.Distance(pointPosition, tree.pointPosition);
            if (currentDistance <= closestDistance)
            {
                closestDistance = currentDistance;
                closestTreeIndex = index;
            }
        }
        
        //test for hitbox in tree so they can be independent 
        Debug.Log($"Closest tree: {_trees[closestTreeIndex].name}");
        _trees[closestTreeIndex].TrySelectTree(closestDistance);
    }
    
    private void CheckProgression()
    {
        foreach (TreeWorldMap tree in _trees)
        {
            bool isUnlocked = _progressionManager.treeIsUnlocked(tree.treeId);
            tree.SetLockedStatus(!isUnlocked);
        }
    }
}
