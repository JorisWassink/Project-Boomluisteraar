using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WorldMapUI : MonoBehaviour
{
    [SerializeField] private string requirementLabelText;
    [SerializeField] private TextMeshProUGUI experienceLabel;
    [SerializeField] private TextMeshProUGUI requirementsLabel;
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private WorldMapUIScreens worldMapUIScreens;

    private GameObject _currentUI;
    private GameManager _gameManager;
    private TreeSelectionManager _treeSelectionManager;
    private ProgressionManager _progressionManager;
    
    private List<GameObject> Screens =>  new List<GameObject>
    {
        worldMapUIScreens.mainMapUI, 
    };

    //Unity Event Functions
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _treeSelectionManager = _gameManager.treeSelectionManager;
        _progressionManager = _gameManager.progressionManager;
        
        _progressionManager.onPointsUpdate.AddListener(UpdateExperienceLabel);
        UpdateExperienceLabel();
        SetDefaultUI();
    }
    
    //Public Functions
    public void SetDefaultUI()
    {
        SetCurrentUI(worldMapUIScreens.mainMapUI);
        _treeSelectionManager.SetUIMode(false);
    }
    
    //Private Functions
    private void UpdateExperienceLabel()
    {
        int points = _progressionManager.playerExperience;
        int goal = _progressionManager.nextExperienceTarget;

        experienceLabel.text = $"{points} /  {goal} XP";
        experienceSlider.value = points / goal;
        requirementsLabel.text = $"{requirementLabelText} {goal}XP";
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
}