using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DataChunk = DialogueDataBase.DialogueChunk;
using ChunkEntry = DialogueDataBase.ChunkEntry;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RawImage characterImage;
    [SerializeField] private UIManager uiManager;
    
    private DialogueEventManager _eventManager;
    private string _currentCharacterId;
    private DataChunk _currentDialogueChunk;
    private int _currentDialogueIndex;
    
    public void SetNewDialogueChunk(Scene scene, string treeId, DialogueType dialogueType)
    {
        Debug.Log("Setting new dialogue chunk");
        DataChunk dialogueChunk = SelectDialogueChunk(scene, treeId, dialogueType);
        DisplayDialogueChunk(dialogueChunk);
    }

    public void SetNewDialogueChunk(DialogueEvent dialogueEvent)
    {
        Debug.Log("Setting event dialogue");
        DataChunk dialogueChunk = StaticVariables.GetDialogueChunk(dialogueEvent);
        DisplayDialogueChunk(dialogueChunk);
    }
    
    public void SetDialogueEventManager(DialogueEventManager eventManager)
    {
        _eventManager = eventManager;
    }

    private void DisplayDialogueChunk(DataChunk dataChunk)
    {
        if (dataChunk == null || dataChunk.chunkEntries.Count == 0)
        {
            Debug.LogWarning("DisplayDialogueChunk was called with an empty data chunk");
            dataChunk = DataChunk.Empty;
        }
        Debug.Log(dataChunk.characterID);
        CheckCharacter(dataChunk.characterID);
        
        foreach (ChunkEntry chunkEntry in dataChunk.chunkEntries)
        {
            if(chunkEntry.isEvent)
                Debug.Log(chunkEntry.command);
            else
                Debug.Log(chunkEntry.dialogueText);
        }
        _currentDialogueChunk = dataChunk;
        _currentDialogueIndex = 0;
        UpdateDialogue();
    }
    

    private DataChunk SelectDialogueChunk(Scene scene, string treeId, DialogueType dialogueType)
    {
        DataChunk dialogueChunk = StaticVariables.GetDialogueChunk(scene, dialogueType, treeId);
        return dialogueChunk;
    }

    private void CheckCharacter(string id)
    {
        if (_currentCharacterId != id || _currentCharacterId == null)
            UpdateCharacter(id);
    }

    public void OnDialogueTapped()
    {
        if (_currentDialogueIndex + 1 >= _currentDialogueChunk.chunkEntries.Count)
            CloseDialogue();
        else
            NextDialogue();
    }
    
    private void CloseDialogue()
    {
        dialogueText.text = "";
        _currentDialogueIndex = 0;
        uiManager.SetDefaultUI();
    }

    private void NextDialogue()
    {
        _currentDialogueIndex++;
        UpdateDialogue();
    }

    private void UpdateCharacter(string id)
    {
        _currentCharacterId = id;
        characterNameText.text = StaticVariables.GetCharacterName(id);
        
        UpdateImage(id);
    }

    private void UpdateImage(string id)
    {
        Texture2D texture = StaticVariables.GetCharacterSprite(id);
        float ratio = (float)texture.width / (float)texture.height;
        
        if (characterImage.gameObject.TryGetComponent(out AspectRatioFitter aspectRatioFitter))
            aspectRatioFitter.aspectRatio = ratio;
        
        characterImage.texture = texture;
    }
    

    private void UpdateDialogue()
    {
        ChunkEntry chunkEntry = _currentDialogueChunk.chunkEntries[_currentDialogueIndex];
        bool isEvent = chunkEntry.isEvent;

        if (isEvent)
        {
            _eventManager.FireEvent(chunkEntry.command);
            OnDialogueTapped();
        }
        else
        {
            dialogueText.text = chunkEntry.dialogueText;
        }
    }
}

public enum DialogueType
{
    Random,
    EventDialogue
    
}