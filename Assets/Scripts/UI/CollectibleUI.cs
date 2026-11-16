using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleUI : MonoBehaviour
{
    [SerializeField] private float horizontalSpacing = 10;
    
    [SerializeField] private TextMeshProUGUI pageNameText;
    [SerializeField] private TextMeshProUGUI discoveredText;
    [SerializeField] private TextMeshProUGUI discriptionText;
    [SerializeField] private TextMeshProUGUI observationText;
    
    [SerializeField] private RawImage image;
    
    [SerializeField] private GameObject pageContentObject;
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    
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

        
        pageNameText.text = isCollected ? collectibleName : "Onbekend object";
        discoveredText.text = isCollected ? "Ontdekt" : "Nog niet ontdekt";
        discriptionText.text = isCollected ? descriptionString : "Nog niet ontdekt";
        observationText.text = observationString;
        
        image.texture = sprite;
        
        CheckButtons(); 
    }

    private void CheckButtons()
    {
        bool canGoLeft = _currentPage > 0;
        bool canGoRight = _currentPage < _pages.Count - 1;
        
        pageLeftButton.gameObject.SetActive(canGoLeft);
        pageRightButton.gameObject.SetActive(canGoRight);
    }
    
}
