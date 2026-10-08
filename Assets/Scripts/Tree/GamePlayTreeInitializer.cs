using UnityEngine;

public class GamePlayTreeInitializer : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;

    void Awake()
    {
        GameObject treeObject = Instantiate(treePrefab,  transform);
        TreeGameplay treeGameplay = treeObject.GetComponent<TreeGameplay>(); 
        treeGameplay.InitializeTree(StaticVariables.SelectedTreeId);
    }
}
