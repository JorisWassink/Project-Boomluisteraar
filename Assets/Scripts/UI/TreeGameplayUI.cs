using System;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[Serializable]
public enum UIMenu
{
    TreeInfo,
    MiniGameMenu,
    DialogueMenu,
    MinigameTest,
    CollectibleMenu
}

public class TreeGameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI treeNameTextDutch;
    [SerializeField] private TextMeshProUGUI treeNameTextLatin;
    
    [SerializeField] private GameObject treeInfoMenu;
    [SerializeField] private GameObject miniGameMenu;
    [SerializeField] private GameObject collectibleMenu;
    [SerializeField] private GameObject dialogueMenu;
    
    private GameObject _treeInfoUI;
    private GameObject _activeUI;
    private List<GameObject> _allUIs = new ();
    
    //Unity Event Functions
    private void Start()
    {
        treeNameTextDutch.text = GameManager.Instance.gameplayManager.currentTreeGameplay.treeNameDutch;
        treeNameTextLatin.text = GameManager.Instance.gameplayManager.currentTreeGameplay.treeNameLatin;
        
        _allUIs.Add(treeInfoMenu);
        _allUIs.Add(miniGameMenu);
        _allUIs.Add(collectibleMenu);
        _allUIs.Add(dialogueMenu);
        
        SetUI(treeInfoMenu);
    }

    //Public Functions
    public void GoToWorldMap()
    {
        GameManager.Instance.SwitchScene(Scene.WorldMap);
    }
    public void ShowTreeInfo() => SetUI(treeInfoMenu);
    public void ShowMiniGameMenu() => SetUI(miniGameMenu);
    public void ShowCollectibleMenu() => SetUI(collectibleMenu);
    public void ShowDialogueMenu() => SetUI(dialogueMenu);
    //Private Functions
    private void SetUI(GameObject menu)
    {
        foreach (GameObject ui in _allUIs)
        {
            ui.SetActive(false);
        }
        menu.SetActive(true);
        _activeUI = menu;
    }
}
