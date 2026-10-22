using UnityEngine;

public class GamePlayTreeInitializer : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;

    void Awake()
    {
        string treeId = StaticVariables.SelectedTreeId;
        if(treeId == null) treeId = "91261114";
        GameObject treeObject = Instantiate(treePrefab,  transform);
        TreeGameplay treeGameplay = treeObject.GetComponent<TreeGameplay>(); 
        treeGameplay.InitializeTree(treeId);
        GameplayManager.Instance.currentTreeGameplay = treeGameplay;
    }
}
