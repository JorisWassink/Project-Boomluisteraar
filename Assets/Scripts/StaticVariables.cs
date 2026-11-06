using System.Collections.Generic;
using UnityEngine;

public static class StaticVariables
{
    static ScriptableObjectDataBase _so;
    static TreeDataBase _treeDataBase;
    static CollectibleDataBase _collectibleDataBase;

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


    
    public static TreeDataBase.TreeEntry GetTreeEntry(string id) => Trees.GetTreeEntry(id);
    public static GameObject GetTreeModel(string treeId) => Trees.GetTreeModel(treeId);
    public static string GetTreeName(string treeId) => Trees.GetTreeName(treeId);
    public static DataType GetTreeType(string treeId) => Trees.GetTreeType(treeId);
    public static List<string> GetCollectibleIdsByTree(string treeId) => Collectibles.GetCollectibleIdsByTree(treeId);
    public static string GetCollectibleName(string collectibleID) => Collectibles.GetCollectibleName(collectibleID);
    public static Texture2D GetCollectibleSprite(string collectibleID) => Collectibles.GetCollectibleSprite(collectibleID);
    public static List<string> GetAllCollectibleIds => Collectibles.GetAllCollectibleIds();
    public static List<string> GetDefaultCollectibleIds => Collectibles.GetDefaultCollectibleIds();
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