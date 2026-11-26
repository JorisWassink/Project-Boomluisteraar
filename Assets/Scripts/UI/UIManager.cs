using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(DialogueEventManager))]
public class UIManager : MonoBehaviour
{
    [SerializeField] protected GameObject defaultUIScreen;
    [SerializeField] protected GameObject dialogueUIScreen;
    
    protected DialogueManager dialogueManager;
    protected GameManager gameManager;
    protected TreeSelectionManager treeSelectionManager;
    protected ProgressionManager progressionManager;


    public List<GameObject> screens = new();

    protected void Start()
    {
        InitializeManagers();
        
        screens.Add(defaultUIScreen);
        screens.Add(dialogueUIScreen);
    }

    private void InitializeManagers()
    {
        gameManager = GameManager.Instance;
        treeSelectionManager = gameManager.treeSelectionManager;
        progressionManager = gameManager.progressionManager;
        
        dialogueManager = dialogueUIScreen.GetComponent<DialogueManager>();
        
        var dem = GetComponent<DialogueEventManager>();
        dialogueManager.SetDialogueEventManager(dem);
    }

    public virtual void SetDefaultUI() { }
    

    protected void SwitchScene(Scene scene)
    {
        Debug.Log($"Switching to {scene}");
        gameManager.SwitchScene(scene);
    }
    
    public virtual void SetRandomDialogueUI()
    {
        SetDialogueUI();
    }
    protected void SetEventDialogue(DialogueEvent dialogueEvent)
    {
        dialogueManager.SetNewDialogueChunk(dialogueEvent);
        SetDialogueUI();    
    }
    
    protected void SetDialogueUI()
    {
        SetCurrentUI(dialogueUIScreen,true, false);
    }

    protected void SetCurrentUI(GameObject currentUI, bool uiMode, bool closePreviousUI = true)
    {
        Debug.Log("Set current UI to " + currentUI.name);
        
        if (closePreviousUI)
            foreach (GameObject ui in screens)
                ui.SetActive(false);
        
        currentUI.SetActive(true);
        treeSelectionManager.SetUIMode(uiMode);
    }
}
