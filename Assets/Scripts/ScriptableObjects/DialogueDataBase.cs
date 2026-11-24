using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "DialogueDataBase", menuName = "Scriptable Objects/DialogueDataBase")]
public class DialogueDataBase : ScriptableObject
{
    [Serializable]
    public class CharacterDialogueEntry
    {
        public string characterName;
        public WorldMapDialogues worldMapDialogues;
        public GamePlayDialogues gamePlayDialogues;
        public List<RandomDialogueListEntry> randomWorldMapDialogueListEntries;
        public List<RandomDialogueListEntry> randomGameplayDialogueListEntries;
        
        [HideInInspector] public CharacterDataBase.CharacterEntry characterEntry;
        [HideInInspector] public int expRequirement;
        
        [HideInInspector] public string treeId;
    }
    
    [Serializable]
    public class EventDialogue
    {
        public DialogueEvent eventType;
        [CharacterSelect] public string character;
        public DialogueChunk dialogues;
        [HideInInspector] public CharacterDataBase.CharacterEntry characterEntry;
        
    }

    [Serializable]
    public class WorldMapDialogues
    {
        public DialogueChunk welcomeDialogue;
    }
    
    [Serializable]
    public class RandomDialogueListEntry
    {
        public DialogueChunk dialogueChunkList;
    }

    [Serializable]
    public class GamePlayDialogues
    {
        public DialogueChunk coldComment;
        public DialogueChunk hotComment;
    }

    [Serializable]
    public class DialogueChunk
    {
        public List<ChunkEntry> chunkEntries;
        [HideInInspector] public string characterID;
        
        public static DialogueChunk Empty => new DialogueChunk
        {
            chunkEntries = new List<ChunkEntry>()
            {
                new (){dialogueText = "NO DIALOGUE FOUND"}
            }
        };
    }

    [Serializable]
    public class ChunkEntry
    {
        public bool isEvent = false;
        public string dialogueText;
        public EventCommands command;
        public CharacterEmotion emotion;
    }

    [SerializeField] CharacterDataBase characterDataBase;
    [SerializeField] Texture2D defaultSprite;

    public List<EventDialogue> eventDialogues = new();
    public List<CharacterDialogueEntry> characterEntries = new();
    

    public DialogueChunk GetDialogueChunk(Scene scene, DialogueType dialogueType, string treeId)
    {
        switch (scene)
        {
            case Scene.TitleScreen:
                return GetTitleScreenDialogueChunk(dialogueType, treeId);
            case Scene.WorldMap:
                return GetWorldMapDialogueChunk(dialogueType, treeId);
            case Scene.TreeView:
                return GetGameplayDialogueChunk(dialogueType, treeId);
            default:
                return DialogueChunk.Empty;
        }
    }

    public DialogueChunk GetEventDialogueChunk(DialogueEvent eventType)
    {
        foreach (var t in eventDialogues)
            if (t.eventType == eventType)
                return t.dialogues;
        
        Debug.LogWarning($"{eventType} dialogue chunk not found");
        return DialogueChunk.Empty;
    }
    
    private DialogueChunk GetTitleScreenDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterDialogueEntry dialogueEntry = GetCharacterDialogueEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(dialogueEntry.randomWorldMapDialogueListEntries);
            default:
                return DialogueChunk.Empty;
        }
    }

    
    private DialogueChunk GetWorldMapDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterDialogueEntry dialogueEntry = GetCharacterDialogueEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(dialogueEntry.randomWorldMapDialogueListEntries);
            default:
                return DialogueChunk.Empty;
        }
    }
    
    private DialogueChunk GetGameplayDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterDialogueEntry dialogueEntry = GetCharacterDialogueEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(dialogueEntry.randomGameplayDialogueListEntries);
            default:
                return DialogueChunk.Empty;
        }
    }

    public DialogueChunk GetRandomDialogueChunk(List<RandomDialogueListEntry> chunkList)
    {
        if (chunkList == null || chunkList.Count == 0)
        {
            return DialogueChunk.Empty;
        }
        
        int index = Random.Range(0, chunkList.Count - 1);
        Debug.Log($"{index} out of {chunkList.Count}");
        return chunkList[index].dialogueChunkList;
    }
    
    public string GetCharacterName(string id)
    {
        CharacterDataBase.CharacterEntry dialogueEntry = GetCharacterEntry(id);
        return dialogueEntry.characterName;
    }
    
    public Texture2D GetCharacterSprite(string id, CharacterEmotion emotion)
    {
        CharacterDataBase.CharacterEntry dialogueEntry = GetCharacterEntry(id);
        Texture2D sprite;
        switch (emotion)
        {
            case CharacterEmotion.Default:
            default:
                sprite = dialogueEntry.sprites.defaultSprite;
                break;
        }
        return sprite;
    }
    
    private CharacterDialogueEntry GetCharacterDialogueEntry(string id)
    {
        foreach (var t in characterEntries)
            if (t.treeId == id) return t;
        
        return null;
    }
    
    private CharacterDataBase.CharacterEntry GetCharacterEntry(string id)
    {
        Debug.Log(characterDataBase.characters);
        if (characterDataBase == null) return null;
        foreach (var ce in characterDataBase.characters)
        {
            Debug.Log(ce.characterName);
        }
        return characterDataBase.characters.Find(c => c.characterId == id);
    }


#if UNITY_EDITOR
    [ContextMenu("Sync With Character Database")]
    void SyncWithCharacters()
    {
        if (characterDataBase == null) return;

        foreach (var character in characterDataBase.characters)
        {
            var entry = characterEntries.Find(e => e.treeId == character.characterId);
            if (entry == null)
            {
                entry = new CharacterDialogueEntry { treeId = character.characterId };
                characterEntries.Add(entry);
            }
            entry.characterEntry = character;
            entry.characterName = character.characterName;
            entry.expRequirement = character.expRequirement;
        }

        
        characterEntries.Sort((a, b) => a.expRequirement.CompareTo(b.expRequirement));

        characterEntries.RemoveAll(e => !characterDataBase.characters.Exists(t => t.characterId == e.treeId));

        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    void SyncEventDialogues()
    {
        var seen = new HashSet<DialogueEvent>();
        eventDialogues.RemoveAll(d => !seen.Add(d.eventType));

        foreach (DialogueEvent e in Enum.GetValues(typeof(DialogueEvent)))
        {
            if (!seen.Contains(e))
                eventDialogues.Add(new EventDialogue { eventType = e });
        }

        eventDialogues.Sort((a, b) => a.eventType.CompareTo(b.eventType));
        UnityEditor.EditorUtility.SetDirty(this);
    }

    void OnValidate()
    {
        SyncWithCharacters();
        SyncEventDialogues();

        foreach (var e in eventDialogues)
        {
            e.dialogues ??= new DialogueChunk { chunkEntries = new List<ChunkEntry>() };
            e.dialogues.characterID = e.character;
            e.characterEntry = GetCharacterEntry(e.character);
        }
    }
    
#endif

}
public enum DialogueEvent
{
    FirstGameStart,
    FirstWorldMapVisit
}

public enum EventCommands
{
    UnlockPlayButton
}
