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
    [SerializeField] private TextMeshProUGUI treeNameText;
    [SerializeField] private Canvas miniGameMenu;
    
    private Canvas _treeInfoCanvas;
    private Canvas _activeCanvas;
    private List<Canvas> _allCanvases = new List<Canvas>();
    


    private void Start()
    {
        if(treeNameText == null) Debug.LogError("TreeGameplayUI: No TreeNameText assigned");
        treeNameText.text = GameplayManager.Instance.currentTreeGameplay.name;
        
        Canvas thisCanvas = GetComponent<Canvas>();
        if(thisCanvas == null)
            Debug.LogError("TreeGameplayUI: Canvas not found");
        else 
            _treeInfoCanvas = thisCanvas;
        
        _allCanvases.Add(_treeInfoCanvas);
        _allCanvases.Add(miniGameMenu);
        SetUI(UIMenu.TreeInfo);
        
    }

    //Button events
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scenes/WorldMap");
    }
    
    public void ShowTreeInfo() => SetUI(UIMenu.TreeInfo);
    public void ShowMiniGameMenu() => SetUI(UIMenu.MiniGameMenu);
    public void ShowDialogueMenu() => SetUI(UIMenu.DialogueMenu);

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
                c.enabled = false;
                continue;
            }
            
            c.enabled = true;
            _activeCanvas = c;
        }
    }
}
