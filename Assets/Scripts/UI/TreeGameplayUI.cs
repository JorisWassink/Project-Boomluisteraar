using System;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class TreeGameplayUI : UIManager
{
    [SerializeField] private TextMeshProUGUI treeNameTextEnglish;
    [SerializeField] private TextMeshProUGUI treeNameTextLatin;
    
    [SerializeField] private GameObject miniGameMenu;
    [SerializeField] private GameObject collectibleMenu;
    
    
    private GameObject _treeInfoUI;
    private GameObject _activeUI;
    
    //Unity Event Functions
    private void Start()
    {
        InitializeManagers();
        treeNameTextEnglish.text = gameManager.gameplayManager.currentTreeGameplay.treeNameEnglish;
        treeNameTextLatin.text = gameManager.gameplayManager.currentTreeGameplay.treeNameLatin;
        
        screens.Add(miniGameMenu);
        screens.Add(collectibleMenu);
        
        SetDefaultUI();
    }

    //Public Functions
    public override void SetDialogueUI(DialogueType dialogueType)
    {
        dialogueManager.DisplayDialogue(Scene.TreeView, progressionManager.currentGuideTreeId, dialogueType);
        base.SetDialogueUI(dialogueType);
    }

    public override void SetDefaultUI() => SetCurrentUI(defaultUIScreen, true);
    public void GoToWorldMap() => gameManager.SwitchScene(Scene.WorldMap);
    public void ShowMiniGameMenu() => gameManager.SwitchScene(Scene.JumpUpMiniGame);
    
    public void ShowTreeInfo() => SetDefaultUI();
    public void ShowCollectibleMenu() => SetCurrentUI(collectibleMenu, true);
    public void ShowDialogueMenu() => SetCurrentUI(dialogueUIScreen, true);

}
