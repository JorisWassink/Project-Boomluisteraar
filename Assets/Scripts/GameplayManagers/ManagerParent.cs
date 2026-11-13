using UnityEngine;

public class ManagerParent : MonoBehaviour
{
    protected GameManager gameManager;
    
    public virtual void Initialize(GameManager gm)
    {
        gameManager = gm;
    }
}
