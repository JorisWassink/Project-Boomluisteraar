using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] protected GameObject defaultUIScreen;
    [SerializeField] protected GameObject dialogueUIScreen;
    
    protected DialogueManager dialogueManager;
    protected GameManager gameManager;
    protected TreeSelectionManager treeSelectionManager;
    protected ProgressionManager progressionManager;


    protected List<GameObject> screens => new List<GameObject>
    {
        defaultUIScreen, 
        dialogueUIScreen
    };

    protected void InitializeManagers()
    {
        gameManager = GameManager.Instance;
        treeSelectionManager = gameManager.treeSelectionManager;
        progressionManager = gameManager.progressionManager;
        
        dialogueManager = dialogueUIScreen.GetComponent<DialogueManager>();
    }

    public virtual void SetDefaultUI() { }

    public void SetRandomDialogueUI()
    {
        SetDialogueUI(DialogueType.Random);
    }
    
    public virtual void SetDialogueUI(DialogueType dialogueType)
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