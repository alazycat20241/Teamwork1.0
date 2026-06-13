using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.GoogleVr;
using UnityEditor;
using UnityEngine;

public class GrowBlock : MonoBehaviour
{
    //状态机
    public enum GrowthStage
    {
        Barren,     // 荒地
        Planted,    // 已播种
        Growing1,   // 生长阶段1
        Growing2,   // 生长阶段2
        Ripe        // 成熟
    }

    public GrowthStage currentStage = GrowthStage.Barren;

    //精灵贴图
    public Sprite barrenSprite;
    public Sprite plantedSprite;
    public Sprite growing1Sprite;
    public Sprite growing2Sprite;
    public Sprite ripeSprite;

    private SpriteRenderer sr;

    //生长时间
    
    public int timeToGrowing1 = 1;
    public int timeToGrowing2 = 3;
    public int timeToRipe = 4;

    private float growTimer = 0f;

    //收获
    public GameObject harvestItemPrefab; // 成熟后掉落的作物
    public Transform harvestSpawnPoint;  // 掉落位置

    //描边效果
    public SpriteRenderer outlineSR;
    public Color normalColor = new Color(1, 1, 1, 0);
    public Color hoverColor = new Color(0, 1, 0, 0.8f);

    //生长时间
    int PlantDay = 1;
    int CurrentDay = 1;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        
        if (harvestSpawnPoint == null)
            harvestSpawnPoint = transform;

        UpdateSprite();


        if (outlineSR == null)
            outlineSR = GetComponent<SpriteRenderer>();

        outlineSR.color = normalColor;
    }

    
    void Update()
    {
        HandleMouseInput();
        HandleHoverHighlight();
        CurrentDay = ActionPointManager.Instance.GetCurrentDay();
        if (PlantDay != CurrentDay)
        {
            AutoGrow();
        }
    }

    //输入
    void HandleMouseInput()
    {

        // 左键：收获
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject) {
                
                if (currentStage == GrowthStage.Barren)
                {
                    TryPloughOrPlant();
                }
                else if (currentStage == GrowthStage.Ripe)
                {
                    Harvest();
                }
            }
        }
    }
    //耕地/犁地
    public void TryPloughOrPlant()
    {
            //记录种植时的时间
            PlantDay = ActionPointManager.Instance.GetCurrentDay();

            // 检测背包是否有种子
            if (PlayerInventory.Instance != null &&
                PlayerInventory.Instance.UseSeed())
            {
                currentStage = GrowthStage.Planted;
                UpdateSprite();
            }
            else
            {
                // 没有种子
                Debug.Log("种子不足，无法种植！");
            }
    }
    //自动生长
    void AutoGrow()
    {
        if (currentStage == GrowthStage.Planted ||
            currentStage == GrowthStage.Growing1 ||
            currentStage == GrowthStage.Growing2)
        {
            if (currentStage == GrowthStage.Planted && PlantDay <= CurrentDay- timeToGrowing1)
            {
                currentStage = GrowthStage.Growing1;
                
            }
            else if (currentStage == GrowthStage.Growing1 && PlantDay <= CurrentDay- timeToGrowing2)
            {
                currentStage = GrowthStage.Growing2;
                
            }
            else if (currentStage == GrowthStage.Growing2 && PlantDay <= CurrentDay- timeToRipe)
            {
                currentStage = GrowthStage.Ripe;
                Debug.Log("11111");
            }

            UpdateSprite();
        }
    }

    //收获
    void Harvest()
    {
        if (harvestItemPrefab != null)
        {
            Instantiate(harvestItemPrefab,
                        harvestSpawnPoint.position + Vector3.up * 0.5f,
                        Quaternion.identity);
        }

        currentStage = GrowthStage.Barren;
        growTimer = 0f;
        UpdateSprite();
    }

    //精灵贴图跟随土地状态更新
    void UpdateSprite()
    {
        if (sr == null) return;

        switch (currentStage)
        {
            case GrowthStage.Barren: sr.sprite = barrenSprite; break;
            case GrowthStage.Planted: sr.sprite = plantedSprite; break;
            case GrowthStage.Growing1: sr.sprite = growing1Sprite; break;
            case GrowthStage.Growing2: sr.sprite = growing2Sprite; break;
            case GrowthStage.Ripe: sr.sprite = ripeSprite; break;
        }
    }

    //描边效果
    public void Show()
    {
        outlineSR.color = hoverColor;
    }

    public void Hide()
    {
        outlineSR.color = normalColor;
    }
    void HandleHoverHighlight()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }
    
}