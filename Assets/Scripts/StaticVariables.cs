using System.Collections.Generic;
using UnityEngine;
using DialogueChunk = DialogueDataBase.DialogueChunk;

public static class StaticVariables
{
    static ScriptableObjectDataBase _so;
    static TreeDataBase _treeDataBase;
    static CollectibleDataBase _collectibleDataBase;
    static DialogueDataBase _dialogueDataBase;
    
    public static bool IsFirstStart = true;

    static ScriptableObjectDataBase SO
    {
        get
        {
            if (_so == null)
            {
                _so = Resources.Load<ScriptableObjectDataBase>("ScriptableObjectDataBase");
                if (_so == null)
                    Debug.LogError("ScriptableObjectDataBase not found in a Resources folder");
            }
            return _so;
        }
    }

    static TreeDataBase Trees => _treeDataBase != null ? _treeDataBase : (_treeDataBase = SO.treeDataBase);
    static CollectibleDataBase Collectibles => _collectibleDataBase != null ? _collectibleDataBase : (_collectibleDataBase = SO.collectibleDataBase);
    private static DialogueDataBase Dialogue => _dialogueDataBase != null ? _dialogueDataBase : (_dialogueDataBase = SO.dialogueDataBase);

    
    public static TreeDataBase.TreeEntry GetTreeEntry(string id) => Trees.GetTreeEntry(id);
    public static GameObject GetTreeModel(string treeId) => Trees.GetTreeModel(treeId);
    public static string GetTreeName(string treeId, bool english) => Trees.GetTreeName(treeId, english);
    public static string GetTreeName(string treeId) => Trees.GetTreeName(treeId, true);
    public static DataType GetTreeType(string treeId) => Trees.GetTreeType(treeId);
    public static int GetTreeExpRequirement(string treeId)  => Trees.GetTreeExpRequirement(treeId);
    
    
    public static List<string> GetCollectibleIdsByTree(string treeId) => Collectibles.GetCollectibleIdsByTree(treeId);
    public static string GetCollectibleName(string collectibleID) => Collectibles.GetCollectibleName(collectibleID);
    public static string GetCollectibleDescription(string collectibleID) => Collectibles.GetCollectibleDescription(collectibleID);
    public static string GetCollectibleObservations(string collectibleID) => Collectibles.GetCollectibleObservations(collectibleID);
    public static Texture2D GetCollectibleSprite(string collectibleID, bool unlocked) => Collectibles.GetCollectibleSprite(collectibleID, unlocked);
    public static string GetCharacterName(string id) => Dialogue.GetCharacterName(id);
    public static Texture2D GetCharacterSprite(string id) => Dialogue.GetCharacterSprite(id);
    public static DialogueChunk GetDialogueChunk(Scene scene, DialogueType dialogueType, string treeId) => Dialogue.GetDialogueChunk(scene, dialogueType, treeId);
    public static DialogueChunk GetDialogueChunk(DialogueEvent eventDialogue) => Dialogue.GetEventDialogueChunk(eventDialogue);

    public static List<string> GetAllCollectibleIds => Collectibles.GetAllCollectibleIds();
    public static List<string> GetTreeIds => Trees.GetTreeIds();
    
    public static string SelectedTreeId{get; set;}
    
    public enum DataType
    {
        None,
        CyberSensor,
        CarbonSensor,
        SoilSensor
    }
}

[System.Serializable]
public class TreeModel
{
    public string id;
    public GameObject prefab;
}