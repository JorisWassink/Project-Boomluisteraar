using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RawImage characterImage;
    [SerializeField] private WorldMapUI worldMapUI;
    
    private string currentCharacterId;
    private List<string> currentDialogueList;
    private int currentDialogueIndex;
    
    public void DialogueOnEnable(string treeId)
    {
        DisplayDialogue(treeId);
    }

    private List<string> SelectDialogueChunk(string treeId, DialogueType dialogueType, int index = 0)
    {
        switch (dialogueType)
        {
            case DialogueType.GuideDialogue:
                if (index == 0)
                    return StaticVariables.GetRandomWorldmapDialogueChunk(treeId);
                else
                //todo: make specific dialogue
                    return StaticVariables.GetRandomWorldmapDialogueChunk(treeId);
                break;
            default:
                return new List<string>() { "Dialogue not found" };
                
        }
    }
    
    private void DisplayDialogue(string treeId)
    {
        if (currentCharacterId != treeId || currentCharacterId == null)
            UpdateCharacter(treeId);
        
        currentDialogueList = SelectDialogueChunk(treeId, DialogueType.GuideDialogue);
        UpdateDialogue();
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
        worldMapUI.SetDefaultUI();
        Debug.Log("Dialogue closed");
    }

    private void NextDialogue()
    {
        currentDialogueIndex++;
        UpdateDialogue();
    }

    private void UpdateCharacter(string id)
    {
        currentCharacterId = id;
        
        characterNameText.text = StaticVariables.GetTreeName(id, true);
        characterImage.texture = StaticVariables.GetCharacterSprite(id);
    }

    private void UpdateDialogue()
    {
        dialogueText.text = currentDialogueList[currentDialogueIndex];
    }
}

public enum DialogueType
{
    GuideDialogue
}