using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "DialogueDataBase", menuName = "Scriptable Objects/DialogueDataBase")]
public class DialogueDataBase : ScriptableObject
{
    [Serializable]
    public class CharacterEntry
    {
        public string treeName;
        public string characterName;
        public Texture2D sprite;
        public WorldMapDialogues worldMapDialogues;
        public GamePlayDialogues gamePlayDialogues;
        public List<RandomDialogueListEntry> randomWorldMapDialogueListEntries;
        public List<RandomDialogueListEntry> randomGameplayDialogueListEntries;
        
        [HideInInspector] public int expRequirement;
        
        [HideInInspector] public string treeId;

        public void SetDefaultSprite(Texture2D tex)
        {
            if (sprite == null)
            {
                sprite = tex;
            }
        }
    }
    
    [Serializable]
    public class EventDialogue
    {
        public DialogueEvent eventType;
        [CharacterSelect] public string character;
        public DialogueChunk dialogues;
        
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
    }

    [SerializeField] TreeDataBase treeDataBase;
    [SerializeField] Texture2D defaultSprite;

    public List<EventDialogue> eventDialogues = new();
    public List<CharacterEntry> characterEntries = new();
    

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
        CharacterEntry entry = GetCharacterEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(entry.randomWorldMapDialogueListEntries);
            default:
                return DialogueChunk.Empty;
        }
    }

    
    private DialogueChunk GetWorldMapDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterEntry entry = GetCharacterEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(entry.randomWorldMapDialogueListEntries);
            default:
                return DialogueChunk.Empty;
        }
    }
    
    private DialogueChunk GetGameplayDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterEntry entry = GetCharacterEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(entry.randomGameplayDialogueListEntries);
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
        CharacterEntry entry = GetCharacterEntry(id);
        return entry.characterName;
    }
    
    public Texture2D GetCharacterSprite(string id)
    {
        CharacterEntry entry = GetCharacterEntry(id);
        return entry.sprite;
    }
    
    private CharacterEntry GetCharacterEntry(string id)
    {
        foreach (var t in characterEntries)
            if (t.treeId == id) return t;
        
        return null;
    }
    


#if UNITY_EDITOR
    [ContextMenu("Sync With Tree Database")]
    void SyncWithTrees()
    {
        if (treeDataBase == null) return;

        foreach (var tree in treeDataBase.trees)
        {
            var entry = characterEntries.Find(e => e.treeId == tree.id);
            if (entry == null)
            {
                entry = new CharacterEntry { treeId = tree.id };
                characterEntries.Add(entry);
            }
            entry.treeName = tree.englishName;
            entry.expRequirement = tree.requiredExp;
        }

        
        characterEntries.Sort((a, b) => a.expRequirement.CompareTo(b.expRequirement));

        characterEntries.RemoveAll(e => !treeDataBase.trees.Exists(t => t.id == e.treeId));

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
        foreach (var c in characterEntries)
        {
            c.SetDefaultSprite(defaultSprite);
        }

        foreach (var e in eventDialogues)
        {
            e.dialogues.characterID = e.character;
        }
        
        SyncEventDialogues();
        SyncWithTrees();
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
