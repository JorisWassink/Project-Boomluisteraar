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
    private new void Start()
    {
        base.Start();
        treeNameTextEnglish.text = gameManager.gameplayManager.currentTreeGameplay.treeNameEnglish;
        treeNameTextLatin.text = gameManager.gameplayManager.currentTreeGameplay.treeNameLatin;
        
        screens.Add(miniGameMenu);
        screens.Add(collectibleMenu);
        
        SetDefaultUI();
    }

    //Public Functions
    protected override void SetDialogueUI(DialogueType dialogueType)
    {
        dialogueManager.SetNewDialogueChunk(Scene.TreeView, progressionManager.currentGuideTreeId, dialogueType);
        base.SetDialogueUI(dialogueType);
    }

    public override void SetDefaultUI() => SetCurrentUI(defaultUIScreen, true);
    public void GoToWorldMap() => SwitchScene(Scene.WorldMap);
    public void ShowMiniGameMenu() => SwitchScene(Scene.JumpUpMiniGame);
    
    public void ShowTreeInfo() => SetDefaultUI();
    public void ShowCollectibleMenu() => SetCurrentUI(collectibleMenu, true);

}
