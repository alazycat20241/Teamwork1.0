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
    public Sprite defaultSprite;      // ← 新增：默认图

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
            PlayerStats.Instance.AddPermanentSpeed(addCount);
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