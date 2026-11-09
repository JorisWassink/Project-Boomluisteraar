using System.Collections.Generic;
using UnityEngine;

public class WorldTreeManager : MonoBehaviour
{
    //TODO: editor tooling maybe?
    private List<TreeWorldMap> _trees = new List<TreeWorldMap>();

    private ProgressionManager progressionManager;
    public static WorldTreeManager Instance;

    private bool _selectionInProgress;

    //Unity Event Functions
    private void Awake()
    {
        /* I dont get this nullcheck, why?
        if (Instance == null)
        {
            Instance = this;
        }
        */

        // Why only check for null without handling duplicate instances?
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple WorldTreeManager instances detected.");
            //you can just destroy&return this if you want (I probs wouldnt even do this)
        }

        Instance = this;
    }
    
    void Start()
    {
        _trees = new List<TreeWorldMap>(GetComponentsInChildren<TreeWorldMap>());
        progressionManager = ProgressionManager.Instance;
        
        //TODO: make this be called when the worldmap gets loaded, for now this works i think
        CheckProgression();

        if (InputManager.Instance == null)
        {
            Debug.LogError("WorldTreeManager: InputManager missing!");
            return;
        }

        InputManager.Instance.onMouseClick.AddListener(OnWorldTap);
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.onMouseClick.RemoveListener(OnWorldTap);
        }

        if (Instance == this) Instance = null;
    }

    private void OnWorldTap(Vector2 screenPosition)
    {
        if (_selectionInProgress)
            return;

        Ray ray = InputManager.Instance.GetPointerRay(
            screenPosition
        );

        TreeWorldMap closestTree = null;
        float closestDistance = float.MaxValue;

        foreach (TreeWorldMap tree in _trees)
        {
            if (tree == null || !tree.IsUnlocked) continue;

            // Project the touch ray onto the horizontal
            // plane at this tree's base height.
            Plane plane = new Plane(Vector3.up, tree.transform.position
            );

            if (!plane.Raycast(ray, out float distanceAlongRay)) continue;

            Vector3 hitPoint = ray.GetPoint(distanceAlongRay);

            Vector3 treePos = tree.transform.position;

            // Compare horizontal distance only.
            float distance = Vector2.Distance(new Vector2(hitPoint.x, hitPoint.z), new Vector2(treePos.x, treePos.z)
            );

            if (distance > tree.HitboxRadius) continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTree = tree;
            }
        }

        if (closestTree == null)
        {
            Debug.Log($"World tap missed trees: {screenPosition}");
            return;
        }

        _selectionInProgress = true;

        Debug.Log($"Selected tree: {closestTree.name}");

        closestTree.SelectTree();
    }


    //private functions
    private void CheckProgression()
    {
        int playerExperience = progressionManager.playerExperience;
        foreach (TreeWorldMap tree in _trees)
        {
            bool shouldBeLocked = playerExperience < tree.requiredExperience;
            tree.SetLockedStatus(shouldBeLocked);
        }
    }
}
