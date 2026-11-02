using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldMapUI : MonoBehaviour
{
    [SerializeField] private string labelText;
    [SerializeField] private TextMeshProUGUI experienceLabel;
    
    private ProgressionManager progressionManager;

    //Unity event functions
    private void Start()
    {
        progressionManager = ProgressionManager.Instance;
        progressionManager.onPointsUpdate.AddListener(UpdateExperienceLabel);
        UpdateExperienceLabel();
    }
    
    //Private functions
    private void UpdateExperienceLabel()
    {
        int points = progressionManager.playerExperience;

        experienceLabel.text = labelText + points;
    }
}
