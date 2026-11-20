using UnityEngine;

public class TreeSelectionManager : ManagerParent
{
    //Public Functions
    public void SetUIMode(bool value)
    {
        gameManager.inputManager.SetUIMode(value);
    }
}
