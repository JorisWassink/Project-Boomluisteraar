using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public TreeSelectionManager treeSelectionManager;

    [HideInInspector] public GameplayManager gameplayManager;
    
    [HideInInspector] public ProgressionManager progressionManager;
    [HideInInspector] public InputManager inputManager;
    [HideInInspector] public Camera currentCamera;

    private List<ManagerParent> worldMapManagers = new();
    private List<ManagerParent> treeViewManagers = new();
    
    private PlayerInput inputComponent;
    
    private Dictionary<Scene, string> sceneNames = new()
    {
        [Scene.TreeView] = "GameplayScene",
        [Scene.WorldMap] = "WorldMap",
    };


    public static GameManager Instance;
    
    //Unity Event Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeManagers();
    }

    private void Start()
    {
        //input manager is initialized in the scene for inspector tooling
        inputManager = InputManager.Instance;
        inputComponent = inputManager.gameObject.GetComponent<PlayerInput>();
    }

    //Public Functions
    public void SwitchScene(Scene scene)
    {
        Debug.Log($"Switching to {scene.ToString()}");
        currentCamera = null;
        List<ManagerParent> managers = new();
        
        if (scene == Scene.WorldMap) managers = worldMapManagers;
        else if (scene == Scene.TreeView) managers = treeViewManagers;
        else Debug.LogError($"Scene {scene.ToString()} is not a valid scene");
        
        LoadScene(scene, managers);
    }

    public void SetCurrentCamera(Camera camera)
    {
        currentCamera = camera;

        if (inputComponent == null && InputManager.Instance != null)
            inputComponent = InputManager.Instance.GetComponent<PlayerInput>();

        if (inputComponent != null)
            inputComponent.camera = camera;
    }
    
    //Private Functions
    private void InitializeManagers()
    {
        progressionManager = gameObject.AddComponent<ProgressionManager>();
        
        treeSelectionManager = gameObject.AddComponent<TreeSelectionManager>();
        
        gameplayManager = gameObject.AddComponent<GameplayManager>();
        
        worldMapManagers.Add(treeSelectionManager);
        
        treeViewManagers.Add(gameplayManager);

        InitializeManagers(worldMapManagers);
        InitializeManagers(treeViewManagers);
        
        SetManagersEnabled(treeViewManagers, false);
    }
    
    private void LoadScene(Scene scene, List<ManagerParent> managers)
    {
        SetManagersEnabled(worldMapManagers, false);
        SetManagersEnabled(treeViewManagers, false);
        
        SceneManager.LoadScene(sceneNames[scene]);
        SetManagersEnabled(managers, true);
    }
    
    //Helpers
    private void SetManagersEnabled(List<ManagerParent> managers, bool enabled)
    {
        foreach (ManagerParent managerParent in managers)
        {
            managerParent.enabled = enabled;
        }
    }

    private void InitializeManagers(List<ManagerParent> managers)
    {
        foreach (ManagerParent managerParent in managers)
        {
            managerParent.Initialize(this);
        }
    }
}

public enum Scene
{
    WorldMap,
    TreeView,
}