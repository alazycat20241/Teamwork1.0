using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class FarmShopController : MonoBehaviour
{
    
    public List<ItemData> itemsForSale = new();

    public GameObject shopItemPrefab;
    public Transform contentParent;

    [Header("农场商店面板")]
    //[SerializeField] private SlidePanel FarmPanel;              // 商店面板
    [SerializeField] private Button backButton;                // 返回按钮

    private void Awake()
    {
        backButton.onClick.AddListener(closeMap);
        
    }

    public void closeMap()
    {
        MapUIManager.Instance.CloseCurrentShop();
    }
    void OnEnable()
    {
        RefreshShop();
    }

    void RefreshShop()
    {
        foreach (Transform t in contentParent)
            Destroy(t.gameObject);

        foreach (var item in itemsForSale)
        {
            var go = Instantiate(shopItemPrefab, contentParent);
            SetupItem(go, item);
        }
    }

    void SetupItem(GameObject go, ItemData item)
    {
        go.transform.Find("Image").GetComponent<Image>().sprite = item.icon;
        go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.itemName;
        go.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = item.price.ToString();

        go.GetComponent<Button>().onClick.AddListener(() =>
        {
            Buy(item);
        });
    }

    void Buy(ItemData item)
    {
        var inv = PlayerInventory.Instance;
        if (inv.playerGold < item.price)
        {
            Debug.Log("金币不够");
            return;
        }
        else
        {
            inv.playerGold -= item.price;
            Debug.Log("购买了：" + item.itemName);

            if (item.itemID == 1)
            {
                PlayerInventory.Instance.seedCount++;
            }
        }
    }
}
