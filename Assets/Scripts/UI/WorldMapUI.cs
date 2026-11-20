using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WorldMapUI : UIManager
{
    [SerializeField] private string requirementLabelText;
    [SerializeField] private TextMeshProUGUI experienceLabel;
    [SerializeField] private TextMeshProUGUI requirementsLabel;
    [SerializeField] private Slider experienceSlider;
    

    //Unity Event Functions
    private void Start()
    {
        InitializeManagers();
        
        progressionManager.onPointsUpdate.AddListener(UpdateExperienceLabel);
        UpdateExperienceLabel();
        SetDefaultUI();
        
        if(progressionManager.IsFirstStart)
            SetDialogueUI(DialogueType.WelcomeDialogue);
        
        progressionManager.IsFirstStart = false;
    }
    
    //Public Functions
    public override void SetDialogueUI(DialogueType dialogueType)
    {
        dialogueManager.DisplayDialogue(Scene.WorldMap, progressionManager.currentGuideTreeId, dialogueType);
        base.SetDialogueUI(dialogueType);
    }
    
    public override void SetDefaultUI() => SetCurrentUI(defaultUIScreen, false);

    //Private Functions
    private void UpdateExperienceLabel()
    {
        int points = progressionManager.playerExperience;
        int goal = progressionManager.nextExperienceTarget;

        experienceLabel.text = $"{points} /  {goal} XP";
        
        if(goal > 0)
            experienceSlider.value = points / goal;
        else
            experienceSlider.value = 1;
        
        requirementsLabel.text = $"{requirementLabelText} {goal}XP";
    }
}