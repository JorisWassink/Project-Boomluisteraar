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
    DialogueMenu
}

public class TreeGameplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI treeNameTextDutch;
    [SerializeField] private TextMeshProUGUI treeNameTextLatin;
    
    [SerializeField] private Canvas miniGameMenu;
    
    private Canvas _treeInfoCanvas;
    private Canvas _activeCanvas;
    private List<Canvas> _allCanvases = new List<Canvas>();
    
    //Unity Event Functions
    private void Start()
    {
        treeNameTextDutch.text = GameManager.Instance.gameplayManager.currentTreeGameplay.treeNameDutch;
        treeNameTextLatin.text = GameManager.Instance.gameplayManager.currentTreeGameplay.treeNameLatin;
        
        Canvas thisCanvas = GetComponent<Canvas>();
        if(thisCanvas == null)
            Debug.LogError("TreeGameplayUI: Canvas not found");
        else 
            _treeInfoCanvas = thisCanvas;
        
        _allCanvases.Add(_treeInfoCanvas);
        _allCanvases.Add(miniGameMenu);
        SetUI(UIMenu.TreeInfo);
        
    }

    //Public Functions
    public void GoToWorldMap()
    {
        GameManager.Instance.SwitchScene(Scene.WorldMap);
    }
    
    public void ShowTreeInfo() => SetUI(UIMenu.TreeInfo);
    public void ShowMiniGameMenu() => SetUI(UIMenu.MiniGameMenu);
    public void ShowDialogueMenu() => SetUI(UIMenu.DialogueMenu);
    
    //Private Functions
    private void SetUI(UIMenu menu)
    {
        Canvas canvas;
        switch (menu)
        {
            case UIMenu.MiniGameMenu:
                canvas = miniGameMenu;
                break;
            
            case UIMenu.TreeInfo:
            default:
                canvas = _treeInfoCanvas;
                break;
        }
        foreach (var c in _allCanvases)
        {
            if (c != canvas)
            {
                c.gameObject.SetActive(false);
                continue;
            }
            
            c.gameObject.SetActive(true);
            _activeCanvas = c;
        }
    }
}
