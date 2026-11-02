using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldMapUI : MonoBehaviour
{
    [SerializeField] private string labelText;
    [SerializeField] private TextMeshProUGUI experienceLabel;
    [SerializeField] private WorldMapUIScreens worldMapUIScreens;

    private GameObject _currentUI;
    private TreeSelectionManager _treeSelectionManager;
    private ProgressionManager _progressionManager;
    
    private List<GameObject> Screens =>  new List<GameObject>
    {
        worldMapUIScreens.mainMapUI, 
        worldMapUIScreens.collectiblesUI
    };

    //Unity Event Functions
    private void Start()
    {
        _treeSelectionManager = TreeSelectionManager.Instance;
        _progressionManager = ProgressionManager.Instance;
        _progressionManager.onPointsUpdate.AddListener(UpdateExperienceLabel);
        UpdateExperienceLabel();
        SetDefaultUI();
    }
    
    //Public Functions
    public void SetCollectiblesUI()
    {
        SetCurrentUI(worldMapUIScreens.collectiblesUI);
        _treeSelectionManager.SetUIMode(true);
    }

    public void SetDefaultUI()
    {
        SetCurrentUI(worldMapUIScreens.mainMapUI);
        _treeSelectionManager.SetUIMode(false);
    }
    
    //Private Functions
    private void UpdateExperienceLabel()
    {
        int points = _progressionManager.playerExperience;

        experienceLabel.text = labelText + points;
    }

    private void SetCurrentUI(GameObject currentUI)
    {
        foreach (GameObject ui in Screens)
        {
            ui.SetActive(false);
        }
        currentUI.SetActive(true);
        _currentUI = currentUI;
    }
}

[Serializable]
public struct WorldMapUIScreens
{
    public GameObject mainMapUI;
    public GameObject collectiblesUI;
}