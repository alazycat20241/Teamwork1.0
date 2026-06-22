using UnityEngine;
using UnityEngine.UI;
using static SaveData;

/// <summary>
/// 最大生命值升级按钮
/// </summary>
public class MaxHealth : MonoBehaviour
{
    public float addCount;                  // 提升数值
    public int GCOUNT;                      // 金币消耗
    public int SCOUNT;                      // 灵魂石消耗
    public Button L;                        // 升级按钮
    public Sprite Select1Sprite;            // 购买后显示的图片
    public Sprite defaultSprite;      // ← 新增：默认图

    private Image img;

    // ===== 存档相关 =====
    public bool IsPurchased { get; private set; }
    public string TechID => GetFullPath();

    void Awake()
    {
        img = GetComponent<Image>();
        if (L != null) L.onClick.AddListener(Click);
    }

    void Click()
    {
        // 如果已经购买，直接返回
        if (IsPurchased) return;

        if (PlayerInventory.Instance.playerGold >= GCOUNT &&
            PlayerInventory.Instance.soulStones >= SCOUNT)
        {
            IsPurchased = true;
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
            PlayerInventory.Instance.playerGold -= GCOUNT;
            PlayerInventory.Instance.soulStones -= SCOUNT;
            PlayerInventory.Instance.UpdateGold();
            PlayerInventory.Instance.UpdateStone();
            PlayerStats.Instance.AddPermanentMaxHealth(addCount);
            if (L != null)
                L.interactable = false;

            // 自动解锁子物体的Unlock组件
            Unlock[] childUnlocks = GetComponentsInChildren<Unlock>(true);
            foreach (var unlock in childUnlocks)
            {
                unlock.ForceUnlock();
            }
        }
    }

    /// <summary> 从存档恢复 </summary>
    public void LoadFromSave(bool purchased)
    {
        if (purchased)
        {
            IsPurchased = true;
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
            if (L != null)
                L.interactable = false;
        }
    }

    /// <summary> 生成存档数据 </summary>
    public TechData GetSaveData()
    {
        return new TechData { techID = TechID, isUnlocked = true, isPurchased = IsPurchased };
    }

    /// <summary> 自动生成唯一ID（路径） </summary>
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
        if (L != null) L.interactable = true;
    }
}