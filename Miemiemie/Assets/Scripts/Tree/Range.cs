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

    private void Update()
    {
        if (IsPurchased)
        {
            // 将按钮图片切换为已升级状态
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
        }
    }

    void Click()
    {
        if (IsPurchased) return;

        if (PlayerInventory.Instance.playerGold >= GCOUNT &&
            PlayerInventory.Instance.soulStones >= SCOUNT)
        {
            IsPurchased = true;
            img.sprite = Select1Sprite;
            PlayerInventory.Instance.SpendGold(GCOUNT);      // 扣除金币
            PlayerInventory.Instance.SpendSoulStones(SCOUNT);      // 扣除灵魂石
            PlayerStats.Instance.AddPermanentRange(addCount);

            // ========== 检测子物体里有没有 Unlock 脚本 ==========
            Unlock unlock = GetComponentInChildren<Unlock>();
            if (unlock != null)
            {
                // 调用 ForceUnlock() 解锁功能
                unlock.ForceUnlock();
            }
        }
    }

    public void LoadFromSave(bool purchased)
    {
        if (purchased)
        {
            IsPurchased = true;
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
            if (r != null)
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