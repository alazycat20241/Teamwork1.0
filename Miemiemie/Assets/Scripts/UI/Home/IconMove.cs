using UnityEngine;
using UnityEngine.UI;

public class IconMove : MonoBehaviour
{
    public enum GrowthStage
    {
        Growing1,
        Growing2,
        Growing3,
        Growing4
    }

    public GrowthStage currentStage = GrowthStage.Growing1;

    public Sprite Select1Sprite;
    public Sprite Select2Sprite;
    public Sprite Select3Sprite;
    public Sprite Select4Sprite;
    public RectTransform targetImageRT;

    public float timeToRipe = 3f;
    public float waitTime = 0f;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        waitTime += 0.1f;


        if (waitTime < timeToRipe)
            return;
        waitTime = 0f;

        switch (currentStage)
        {
            case GrowthStage.Growing1:
                currentStage = GrowthStage.Growing2;
                targetImageRT.anchoredPosition += new Vector2(0, 5f);
                break;
            case GrowthStage.Growing2:
                currentStage = GrowthStage.Growing3;
                targetImageRT.anchoredPosition += new Vector2(0, 5f);
                break;
            case GrowthStage.Growing3:
                currentStage = GrowthStage.Growing4;
                targetImageRT.anchoredPosition -= new Vector2(0, 5f);
                break;
            case GrowthStage.Growing4:
                currentStage = GrowthStage.Growing1;
                targetImageRT.anchoredPosition -= new Vector2(0, 5f);
                break;
        }

        UpdateSprite();

    }

   

    void UpdateSprite()
    {
        if (img == null) return;

        switch (currentStage)
        {
            case GrowthStage.Growing1: img.sprite = Select1Sprite; break;
            case GrowthStage.Growing2: img.sprite = Select2Sprite; break;
            case GrowthStage.Growing3: img.sprite = Select3Sprite; break;
            case GrowthStage.Growing4: img.sprite = Select4Sprite; break;
        }
    }
}