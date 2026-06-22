using UnityEngine;
using UnityEngine.UI;
using static SaveData;

/// <summary>
/// 移动速度升级按钮
/// </summary>
public class Speed : MonoBehaviour
{
    public float addCount;
    public int GCOUNT;
    public int SCOUNT;
    public Button L;
    public Sprite Select1Sprite;
    private Image img;
    public Sprite defaultSprite;      

    public bool IsPurchased { get; private set; }
    public string TechID => GetFullPath();

    void Awake()
    {
        img = GetComponent<Image>();
        if (L != null) L.onClick.AddListener(Click);
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
            PlayerStats.Instance.AddPermanentSpeed(addCount);

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
            if (L != null)
                L.interactable = false;
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
        if (L != null) L.interactable = true;
    }
}