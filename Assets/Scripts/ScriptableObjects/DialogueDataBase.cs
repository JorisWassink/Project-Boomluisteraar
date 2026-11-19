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
        public string id;
        public Texture2D sprite;
        public List<DialogueListEntry> worldMapDialogueEntries;
        public int expRequirement;
        
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
    public class DialogueListEntry
    {
        public List<string> dialogueChunkList;
    }

    [SerializeField] TreeDataBase treeDataBase;
    [SerializeField] Texture2D defaultSprite;
    
    public List<CharacterEntry> characterEntries = new();

    public List<string> GetRandomWorldmapGuideDialogueChunk(string treeId)
    {
        CharacterEntry character = GetCharacterEntry(treeId);
        List<DialogueListEntry> chunkList = character.worldMapDialogueEntries;

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
}
