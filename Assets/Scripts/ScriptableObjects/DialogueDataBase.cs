using System;
using System.Collections.Generic;
using UnityEngine;
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
    public class WorldMapDialogues
    {
        public List<string> welcomeDialogue;
    }
    
    [Serializable]
    public class RandomDialogueListEntry
    {
        public List<string> dialogueChunkList;
    }

    [Serializable]
    public class GamePlayDialogues
    {
        public List<string> coldComment;
        public List<string> hotComment;
    }

    [SerializeField] TreeDataBase treeDataBase;
    [SerializeField] Texture2D defaultSprite;
    
    public List<CharacterEntry> characterEntries = new();
    

    public List<string> GetDialogueChunk(Scene scene, DialogueType dialogueType, string treeId)
    {
        switch (scene)
        {
            case Scene.WorldMap:
                return GetWorldMapDialogueChunk(dialogueType, treeId);
            case Scene.TreeView:
                return GetGameplayDialogueChunk(dialogueType, treeId);
            default:
                return new List<string>(){"Scene not found"};
        }
    }

    private List<string> GetWorldMapDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterEntry entry = GetCharacterEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(entry.randomWorldMapDialogueListEntries);
            case DialogueType.WelcomeDialogue:
                return entry.worldMapDialogues.welcomeDialogue;
            default:
                return new List<string>(){"No dialogue chunk found"};
        }
    }
    
    private List<string> GetGameplayDialogueChunk(DialogueType dialogueType, string treeId)
    {
        CharacterEntry entry = GetCharacterEntry(treeId);
        switch (dialogueType)
        {
            case DialogueType.Random:
                return GetRandomDialogueChunk(entry.randomGameplayDialogueListEntries);
            default:
                return new List<string>(){"No dialogue chunk found"};
        }
    }

    public List<string> GetRandomDialogueChunk(List<RandomDialogueListEntry> chunkList)
    {
        if (chunkList == null || chunkList.Count == 0)
        {
            return new List<string>(){"No dialogue chunk found"};
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

    void OnValidate()
    {
        foreach (var c in characterEntries)
        {
            c.SetDefaultSprite(defaultSprite);
        }
        
        SyncWithTrees();
    }
#endif

    public enum WorldMapDialogueType
    {
        Welcome,
    }
}
