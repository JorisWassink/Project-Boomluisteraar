using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldMapUI : MonoBehaviour
{
    [SerializeField] private string labelText;
    [SerializeField] private TextMeshProUGUI experienceLabel;

    //Unity event functions
    private void Start()
    {
        UpdateExperienceLabel();
    }
    
    //Private functions
    private void UpdateExperienceLabel()
    {
        ProgressionManager progressionManager = ProgressionManager.Instance;
        int points = progressionManager.PlayerExperience;

        experienceLabel.text = labelText + points;
    }
}
