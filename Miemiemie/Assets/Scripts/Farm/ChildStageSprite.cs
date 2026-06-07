using UnityEngine;

[ExecuteAlways]
public class ChildStageSprite : MonoBehaviour
{
    [Header("父物体 GrowBlock")]
    public GrowBlock growBlock;

    [Header("各阶段对应 Sprite")]
    public Sprite spriteBarren;
    public Sprite spriteGrowing1;
    public Sprite spriteGrowing2;
    public Sprite spriteRipe;

    private SpriteRenderer sr;
    private GrowBlock.GrowthStage lastStage;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (growBlock == null)
            growBlock = GetComponentInParent<GrowBlock>();
    }

    void LateUpdate()
    {
        if (growBlock == null || sr == null)
            return;

        // 只有阶段变化时才更新（性能友好）
        if (growBlock.currentStage != lastStage)
        {
            lastStage = growBlock.currentStage;
            UpdateSprite();
        }
    }

    void UpdateSprite()
    {
        switch (growBlock.currentStage)
        {
            case GrowBlock.GrowthStage.Barren:
                sr.sprite = spriteBarren;
                break;

            case GrowBlock.GrowthStage.Planted:
                // 如果没有专门图片，可以复用 barren
                sr.sprite = spriteBarren;
                break;

            case GrowBlock.GrowthStage.Growing1:
                sr.sprite = spriteGrowing1;
                break;

            case GrowBlock.GrowthStage.Growing2:
                sr.sprite = spriteGrowing2;
                break;

            case GrowBlock.GrowthStage.Ripe:
                sr.sprite = spriteRipe;
                break;
        }
    }
}
