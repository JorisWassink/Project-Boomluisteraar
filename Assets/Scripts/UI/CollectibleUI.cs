using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleUI : MonoBehaviour
{
    [SerializeField] private float horizontalSpacing = 10;
    
    [SerializeField] private TextMeshProUGUI pageNameText;
    [SerializeField] private GameObject pageContentObject;
    [SerializeField] private Button pageLeftButton;
    [SerializeField] private Button pageRightButton;
    
    [SerializeField] private GameObject collectiblePrefab;
    
    private ProgressionManager _progressionManager;
    private List<string> _treeIds;
    private List<string> _pages = new();
    
    private int _currentPage = 0;
    
    
    //Unity Event Functions
    private void Awake()
    {
        _progressionManager = ProgressionManager.Instance;
        _pages.Add("default");
    }
    
    private void Start()
    {
        _treeIds = StaticVariables.GetTreeIds;
        _pages.AddRange(_treeIds);
        
        pageLeftButton.onClick.AddListener(() => SetPage(_currentPage - 1));
        pageRightButton.onClick.AddListener(() => SetPage(_currentPage + 1));
 
        SetPage(_currentPage);
    }
    
    //Private Functions
    private void SetPage(int pageNumber)
    {
        if(pageNumber >= _pages.Count || pageNumber < 0) return;
        
        _currentPage =  pageNumber;
        
        List<string> collectibleIds = new();
        string pageId = _pages[pageNumber];
        string pageName = "";
        
        switch (pageId)
        {
            case "default":
                collectibleIds = StaticVariables.GetDefaultCollectibleIds;
                pageName = "General Collectibles";
                break;
            default:
                string treeId = pageId;
                collectibleIds = StaticVariables.GetCollectibleIdsByTree(treeId);
                pageName = $"{StaticVariables.GetTreeName(treeId)} Collectibles";
                break;
        }
        
        pageNameText.text = pageName;
        
        FillPage(collectibleIds);
        CheckButtons(); 
    }

    private void CheckButtons()
    {
        bool canGoLeft = _currentPage > 0;
        bool canGoRight = _currentPage < _pages.Count - 1;
        
        pageLeftButton.gameObject.SetActive(canGoLeft);
        pageRightButton.gameObject.SetActive(canGoRight);
    }

    private void FillPage(List<string> ids)
    {
        //clear page
        while (pageContentObject.transform.childCount > 0) {
            DestroyImmediate(pageContentObject.transform.GetChild(0).gameObject);
        }

        GameObject row = null;
        int rowIndex = 0;
        
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            if(i % 3  == 0)
            {
                rowIndex++;
                row = MakeRow(pageContentObject.transform, rowIndex);
            }

            if (row != null)
            {
                bool locked = !_progressionManager.CollectibleIsUnlocked(id);
                MakeCollectible(row.transform, id, locked);
            }
        }
    }
    
    //Helpers
    private GameObject MakeRow(Transform parent, int rowIndex)
    {
        GameObject row = new GameObject();
        row.AddComponent<RectTransform>();
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandHeight =  true;
        hlg.childForceExpandWidth = true;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;
        hlg.spacing = horizontalSpacing;
        row.transform.SetParent(parent);
        row.name = $"Row{rowIndex}";
        return row;
    }
    
    private void MakeCollectible(Transform row, string id, bool locked)
    {
        string collectibleName = StaticVariables.GetCollectibleName(id);
        Texture2D collectibleSprite = StaticVariables.GetCollectibleSprite(id);
        
        GameObject obj = Instantiate(collectiblePrefab, row);
        obj.name = collectibleName;
        
        RawImage rawImage = obj.GetComponentInChildren<RawImage>();
        rawImage.texture = collectibleSprite;
        
        TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>();
        text.text = collectibleName;

        if (locked)
        {
            //very bad temporary solution
            GameObject greyFilter = new();
            RectTransform rt = greyFilter.AddComponent<RectTransform>();
            rt.SetParent(obj.transform);
            rt.anchoredPosition  = new Vector2(0, 0);
            RawImage image = greyFilter.AddComponent<RawImage>();
            Color grey = Color.grey;
            grey.a = .5f;
            image.color = grey;
        }
    }
    
}
