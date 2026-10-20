using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TreeWorldMap : TreeInstance
{
    [SerializeField] private string id = "9B261005";
    
    private void Awake()
    {
        InitializeTree(id);
    }

    private void Start()
    {
        InputManager.Instance.OnMouseClick.AddListener(() => OnTap());
    }

    private void OnTap()
    {
        Vector3 pointOnFloor = InputManager.Instance.GetMousePositionInWorld();
        float distance =  Vector3.Distance(pointOnFloor, transform.position);
        if (distance < 10)
        {
            StaticVariables.SelectedTreeId = treeId;
            SceneManager.LoadScene("GameplayScene");
        }

    }
}
