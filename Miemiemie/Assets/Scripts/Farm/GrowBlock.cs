using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GrowBlock : MonoBehaviour
{
    //状态机
    public enum GrowthStage
    {
        Barren,     // 荒地
        Ploughed,   // 已犁地
        Planted,    // 已播种
        Growing1,   // 生长阶段1
        Growing2,   // 生长阶段2
        Ripe        // 成熟
    }

    public GrowthStage currentStage = GrowthStage.Barren;

    //精灵贴图
    public Sprite barrenSprite;
    public Sprite ploughedSprite;
    public Sprite plantedSprite;
    public Sprite growing1Sprite;
    public Sprite growing2Sprite;
    public Sprite ripeSprite;

    private SpriteRenderer sr;

    //生长时间
    public float timeToGrowing1 = 3f;
    public float timeToGrowing2 = 3f;
    public float timeToRipe = 3f;

    private float growTimer = 0f;

    //收获
    public GameObject harvestItemPrefab; // 成熟后掉落的作物
    public Transform harvestSpawnPoint;  // 掉落位置

    //描边效果
    public SpriteRenderer outlineSR;
    public Color normalColor = new Color(1, 1, 1, 0);
    public Color hoverColor = new Color(0, 1, 0, 0.8f);


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
        AutoGrow();
        HandleHoverHighlight();


    }

    //输入
    void HandleMouseInput()
    {
        // E键：犁地 / 播种
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null &&
                hit.collider.gameObject == gameObject)
            {
                TryPloughOrPlant();
            }
        }

        // 左键：收获
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null &&
                hit.collider.gameObject == gameObject &&
                currentStage == GrowthStage.Ripe)
            {
                Harvest();
            }
        }
    }
    //耕地/犁地
    public void TryPloughOrPlant()
    {
        if (currentStage == GrowthStage.Barren)
        {
            currentStage = GrowthStage.Ploughed;
        }
        else if (currentStage == GrowthStage.Ploughed)
        {
            currentStage = GrowthStage.Planted;
            growTimer = 0f;
        }

        UpdateSprite();
    }
    //自动生长
    void AutoGrow()
    {
        if (currentStage == GrowthStage.Planted ||
            currentStage == GrowthStage.Growing1 ||
            currentStage == GrowthStage.Growing2)
        {
            growTimer += Time.deltaTime;

            if (currentStage == GrowthStage.Planted && growTimer >= timeToGrowing1)
            {
                currentStage = GrowthStage.Growing1;
                growTimer = 0f;
            }
            else if (currentStage == GrowthStage.Growing1 && growTimer >= timeToGrowing2)
            {
                currentStage = GrowthStage.Growing2;
                growTimer = 0f;
            }
            else if (currentStage == GrowthStage.Growing2 && growTimer >= timeToRipe)
            {
                currentStage = GrowthStage.Ripe;
                growTimer = 0f;
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

        currentStage = GrowthStage.Ploughed;
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
            case GrowthStage.Ploughed: sr.sprite = ploughedSprite; break;
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