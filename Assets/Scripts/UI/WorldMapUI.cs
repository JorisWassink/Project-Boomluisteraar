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

    private DialogueManager _dialogueManager;
    private GameManager _gameManager;
    private TreeSelectionManager _treeSelectionManager;
    private ProgressionManager _progressionManager;
    
    private List<GameObject> Screens =>  new List<GameObject>
    {
        worldMapUIScreens.mainMapUI, 
        worldMapUIScreens.dialogueUI
    };

    //Unity Event Functions
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _treeSelectionManager = _gameManager.treeSelectionManager;
        _progressionManager = _gameManager.progressionManager;
        
        //todo: less hardcody
        _dialogueManager = worldMapUIScreens.dialogueUI.gameObject.GetComponent<DialogueManager>();
        
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

    public void OpenDialogue()
    {
        SetCurrentUI(worldMapUIScreens.dialogueUI, false);
        _dialogueManager.DialogueOnEnable(_progressionManager.currentGuideTreeId);
        _treeSelectionManager.SetUIMode(true);
    }
    
    //Private Functions
    private void UpdateExperienceLabel()
    {
        int points = _progressionManager.playerExperience;
        int goal = _progressionManager.nextExperienceTarget;

        experienceLabel.text = $"{points} /  {goal} XP";
        
        if(goal > 0)
            experienceSlider.value = points / goal;
        else
            experienceSlider.value = 1;
        
        requirementsLabel.text = $"{requirementLabelText} {goal}XP";
    }

    private void SetCurrentUI(GameObject currentUI, bool closePreviousUI = true)
    {
        Debug.Log("Set current UI to " + currentUI.name);
        if (closePreviousUI)
            foreach (GameObject ui in Screens)
                ui.SetActive(false);
        
        currentUI.SetActive(true);
    }
}

[Serializable]
public struct WorldMapUIScreens
{
    public GameObject mainMapUI;
    public GameObject dialogueUI;
}