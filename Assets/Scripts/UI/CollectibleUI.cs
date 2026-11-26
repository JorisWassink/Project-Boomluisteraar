using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI unlockedText;
    [SerializeField] private Slider slider;
    
    [SerializeField] private TextMeshProUGUI treeNameText;
    [SerializeField] private TextMeshProUGUI pageNameText;
    [SerializeField] private TextMeshProUGUI discoveredText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI observationText;
    
    [SerializeField] private RawImage image;
    
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    [SerializeField] private TextMeshProUGUI pageText;
    
    [SerializeField] private GameObject collectiblePrefab;
    
    private GameManager _gameManager;
    private ProgressionManager _progressionManager;
    
    private List<string> _collectibleIds;
    private List<string> _pages = new();

    private string _currentTreeId;
    private int _currentPage = 0;
    
    
    //Unity Event Functions
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _progressionManager = _gameManager.progressionManager;

        _currentTreeId = StaticVariables.SelectedTreeId;
        _collectibleIds = StaticVariables.GetCollectibleIdsByTree(_currentTreeId);
        
        _pages.AddRange(_collectibleIds);
        
        pageLeftButton.onClick.AddListener(() => SetPage(_currentPage - 1));
        pageRightButton.onClick.AddListener(() => SetPage(_currentPage + 1));
 
        SetPage(_currentPage);
        
        treeNameText.text = StaticVariables.GetTreeName(_currentTreeId);
    }
    
    //Private Functions
    private void SetPage(int pageNumber)
    {
        if(pageNumber >= _pages.Count || pageNumber < 0) return;
        
        _currentPage =  pageNumber;
        
        string collectibleId = _pages[pageNumber];
        bool isCollected =  _progressionManager.CollectibleIsUnlocked(collectibleId);
        
        string collectibleName = StaticVariables.GetCollectibleName(collectibleId);
        string descriptionString = StaticVariables.GetCollectibleDescription(collectibleId);
        string observationString = StaticVariables.GetCollectibleObservations(collectibleId);
        
        Texture2D sprite = StaticVariables.GetCollectibleSprite(collectibleId, isCollected);

        
        pageNameText.text = isCollected ? collectibleName : "Unknown collectable";
        discoveredText.text = isCollected ? "Discovered" : "Undiscovered";
        descriptionText.text = isCollected ? descriptionString : "This collectible is not discovered yet";
        observationText.text = observationString;
        
        image.texture = sprite;
        SetProgress();
        CheckButtons(); 
    }

    private void SetProgress()
    {
        int total =  _pages.Count;
        int unlocked = 0;
        foreach (var id in _pages)
        {
            if (_gameManager.progressionManager.CollectibleIsUnlocked(id))
            {
                unlocked++;
            }
        }
        unlockedText.text = $"{unlocked} of {total} found";
        slider.value = (float)unlocked/(float)total;
    }

    private void CheckButtons()
    {
        bool canGoLeft = _currentPage > 0;
        bool canGoRight = _currentPage < _pages.Count - 1;
        
        pageLeftButton.gameObject.SetActive(canGoLeft);
        pageRightButton.gameObject.SetActive(canGoRight);

        pageText.text = $"Entry  {_currentPage + 1} of {_pages.Count}";
    }
    
}
