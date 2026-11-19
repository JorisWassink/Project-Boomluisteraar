using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI treeNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RawImage characterImage;
    [SerializeField] private UIManager uiManager;
    
    private string currentCharacterId;
    private List<string> currentDialogueList;
    private int currentDialogueIndex;
    
    public void DisplayDialogue(Scene scene, string treeId, DialogueType dialogueType)
    {
        if (currentCharacterId != treeId || currentCharacterId == null)
            UpdateCharacter(treeId);
        
        currentDialogueList = SelectDialogueChunk(scene, treeId, dialogueType);
        UpdateDialogue();
    }
    
    private List<string> SelectDialogueChunk(Scene scene, string treeId, DialogueType dialogueType)
    {
        List<string> dialogueChunk = StaticVariables.GetDialogueChunk(scene, dialogueType, treeId);
        return dialogueChunk;
    }

    public void OnDialogueTapped()
    {
        if (currentDialogueIndex + 1 >= currentDialogueList.Count)
            CloseDialogue();
        else
            NextDialogue();
    }
    
    private void CloseDialogue()
    {
        dialogueText.text = "";
        currentDialogueIndex = 0;
        uiManager.SetDefaultUI();
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;
        UpdateDialogue();
    }

    private void UpdateCharacter(string id)
    {
        currentCharacterId = id;
        
        characterNameText.text = StaticVariables.GetCharacterName(id);
        characterImage.texture = StaticVariables.GetCharacterSprite(id);
        treeNameText.text = StaticVariables.GetTreeName(id);
    }

    private void UpdateDialogue()
    {
        dialogueText.text = currentDialogueList[currentDialogueIndex];
    }
}

public enum DialogueType
{
    Random,
    WelcomeDialogue
}