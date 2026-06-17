using UnityEngine;
using UnityEngine.UI;
using static SaveData;

/// <summary>
/// 攻击范围升级按钮
/// </summary>
public class Range : MonoBehaviour
{
    public float addCount;
    public int GCOUNT;
    public int SCOUNT;
    public Button r;
    public Sprite Select1Sprite;
    private Image img;
    public Sprite defaultSprite;      // ← 新增：默认图

    public bool IsPurchased { get; private set; }
    public string TechID => GetFullPath();

    void Awake()
    {
        img = GetComponent<Image>();
        if (r != null) r.onClick.AddListener(Click);
    }

    void Click()
    {
        if (PlayerInventory.Instance.playerGold >= GCOUNT &&
            PlayerInventory.Instance.soulStones >= SCOUNT)
        {
            IsPurchased = true;
            img.sprite = Select1Sprite;
            PlayerInventory.Instance.playerGold -= GCOUNT;
            PlayerInventory.Instance.soulStones -= SCOUNT;
            PlayerStats.Instance.AddPermanentRange(addCount);
            r.interactable = false;
        }
    }

    public void LoadFromSave(bool purchased)
    {
        if (purchased)
        {
            IsPurchased = true;
            img.sprite = Select1Sprite;
            r.interactable = false;
        }
    }

    public TechData GetSaveData()
    {
        return new TechData { techID = TechID, isUnlocked = true, isPurchased = IsPurchased };
    }

    string GetFullPath()
    {
        string path = gameObject.name;
        Transform t = transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }

    public void ResetState()
    {
        IsPurchased = false;
        if (img != null && defaultSprite != null) img.sprite = defaultSprite;        // 恢复默认图
        if (r != null) r.interactable = true;
    }
}