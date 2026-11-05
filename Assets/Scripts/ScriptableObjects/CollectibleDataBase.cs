using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleDataBase", menuName = "Scriptable Objects/CollectibleDataBase")]
public class CollectibleDataBase : ScriptableObject
{
    [Serializable]
    public class CollectibleEntry
    {
        public string name;
        public Texture2D sprite;

        [SerializeField, HideInInspector] string id;
        public string Id => id;

        public void EnsureUniqueId(HashSet<string> seen)
        {
            if (string.IsNullOrEmpty(id) || !seen.Add(id))
            {
                id = Guid.NewGuid().ToString();
                seen.Add(id);
            }
        }

        public void SetDefaultSprite(Texture2D defaultSprite)
        {
            if (sprite == null) sprite = defaultSprite;
        }
    }

    [Serializable]
    public class CollectibleListEntry
    {
        public string treeName;
        public string treeId;
        public List<CollectibleEntry> collectibles = new();
    }

    [SerializeField] TreeDataBase treeDataBase;
    [SerializeField] Texture2D defaultSprite;

    public List<CollectibleListEntry> treeCollectibles = new();
    public List<CollectibleEntry> defaultCollectibles = new();
    
    private List<CollectibleEntry> allCollectibles = new();
    
    public List<string> GetCollectibleIdsByTree(string treeId)
    {
        foreach (var e in treeCollectibles)
            if (e.treeId == treeId) return e.collectibles.ConvertAll(t => t.Id);
        return new List<string>();
    }
    
    public List<string> GetAllCollectibleIds()
    {
        return allCollectibles.ConvertAll(t => t.Id);
    }
    
    public List<string> GetDefaultCollectibleIds()
    {
        return defaultCollectibles.ConvertAll(t => t.Id);
    }

    public string GetCollectibleName(string collectibleId)
    {
        return GetCollectibleEntry(collectibleId).name;
    }
    
    public Texture2D GetCollectibleSprite(string collectibleId)
    {
        return GetCollectibleEntry(collectibleId).sprite;
    }
    

    public CollectibleEntry GetCollectibleEntry(string id)
    {
        foreach (var t in allCollectibles)
            if (t.Id == id) return t;
        return null;
    }
    
#if UNITY_EDITOR
    [ContextMenu("Sync With Tree Database")]
    void SyncWithTrees()
    {
        if (treeDataBase == null) return;

        foreach (var tree in treeDataBase.trees)
        {
            var entry = treeCollectibles.Find(e => e.treeId == tree.id);
            if (entry == null)
            {
                entry = new CollectibleListEntry { treeId = tree.id };
                treeCollectibles.Add(entry);
            }
            entry.treeName = tree.name; // keeps the label in sync after renames
        }

        treeCollectibles.RemoveAll(e => !treeDataBase.trees.Exists(t => t.id == e.treeId));

        UnityEditor.EditorUtility.SetDirty(this);
    }

    void OnValidate()
    {
        var seen = new HashSet<string>();

        foreach (var c in defaultCollectibles)
        {
            c.EnsureUniqueId(seen);
            c.SetDefaultSprite(defaultSprite);
        }

        foreach (var list in treeCollectibles)
        foreach (var c in list.collectibles)
        {
            c.EnsureUniqueId(seen);
            c.SetDefaultSprite(defaultSprite);
        }
        
        SyncWithTrees();

        allCollectibles.Clear();
        allCollectibles.AddRange(defaultCollectibles);
        foreach (var list in treeCollectibles)
        {
            allCollectibles.AddRange(list.collectibles);
        }
    }
#endif
}