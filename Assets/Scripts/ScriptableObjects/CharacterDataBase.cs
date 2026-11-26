using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CharacterDataBase", menuName = "Scriptable Objects/CharacterDataBase")]
public class CharacterDataBase : ScriptableObject
{
    [Serializable]
    public class CharacterEntry
    {
        public string treeName;
        public string characterName;
        public SpriteListEntry sprites;

        
        [HideInInspector] public int expRequirement;
        
         public string characterId;

        public void SetDefaultSprite(Texture2D defaultSprite)
        {
            if (sprites.defaultSprite == null) sprites.defaultSprite = defaultSprite;
            if (sprites.grumpySprite == null) sprites.grumpySprite = sprites.defaultSprite;
            if (sprites.happySprite == null) sprites.happySprite = sprites.defaultSprite;
            

            if (characterName == "")
            {
                characterName = treeName;
            }
        }
    }

    [Serializable]
    public class SpriteListEntry
    {
        public Texture2D defaultSprite;
        public Texture2D happySprite;
        public Texture2D grumpySprite;
    }
    [SerializeField] TreeDataBase treeDataBase;
    [SerializeField] Texture2D defaultSprite;

    public List<CharacterEntry> characters;
    
#if UNITY_EDITOR
    [ContextMenu("Sync With Tree Database")]
    void SyncWithTrees()
    {
        if (treeDataBase == null) return;

        foreach (var tree in treeDataBase.trees)
        {
            var entry = characters.Find(e => e.characterId == tree.id);
            if (entry == null)
            {
                entry = new CharacterEntry() { characterId = tree.id };
                characters.Add(entry);
            }
            entry.treeName = tree.englishName;
            entry.expRequirement = tree.requiredExp;
        }

        
        characters.Sort((a, b) => a.expRequirement.CompareTo(b.expRequirement));

        characters.RemoveAll(e => !treeDataBase.trees.Exists(t => t.id == e.characterId));

        UnityEditor.EditorUtility.SetDirty(this);
    }
    

    void OnValidate()
    {
        foreach (var entry in characters)
        {
            entry.SetDefaultSprite(defaultSprite);
        }
        SyncWithTrees();
    }
#endif
}

public enum CharacterEmotion
{
    Default,
    Happy,
    Grumpy,
}