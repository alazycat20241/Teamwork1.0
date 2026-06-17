using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmShopController : MonoBehaviour
{
    
    public List<ItemData> itemsForSale = new();

    public GameObject shopItemPrefab;
    public Transform contentParent;

    [Header("农场商店面板")]
    [SerializeField] private Button backButton;                // 返回按钮

    [Header("提示框")]
    [SerializeField] private GameObject tipPanel;              // 提示框面板（包含底图和文字）
    [SerializeField] private TMP_Text tipText;                 // 提示文字
    [SerializeField] private float tipDisplayDuration = 1.5f;  // 提示显示时长

    private Coroutine tipCoroutine;  // 提示框协程

    private void Awake()
    {
        backButton.onClick.AddListener(closeMap);

        // ★ 初始隐藏提示框
        if (tipPanel != null)
            tipPanel.SetActive(false);
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
        go.transform.Find("Outline").GetComponent<Image>().sprite = item.icon;
        go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.itemName;
        go.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = item.price.ToString();
        go.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            Buy(item);
        });
    }

    void Buy(ItemData item)
    {
        var inv = PlayerInventory.Instance;
        if (inv.playerGold < item.price)
        {
            // ★ 显示金币不够提示
            ShowTip("金币不够");
            return;
        }
        else
        {
            inv.playerGold -= item.price;

            // ★ 显示购买成功提示
            ShowTip("购买了：" + item.itemName);

            if (item.itemID == 1)
            {
                PlayerInventory.Instance.AddSeed(1);
            }
        }
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    /// <param name="message">提示文字内容</param>
    private void ShowTip(string message)
    {
        // 停止之前的提示协程（避免叠加）
        if (tipCoroutine != null)
            StopCoroutine(tipCoroutine);

        tipCoroutine = StartCoroutine(ShowTipCoroutine(message));
    }

    /// <summary>
    /// 提示框显示协程：显示 -> 等待 -> 隐藏
    /// </summary>
    private IEnumerator ShowTipCoroutine(string message)
    {
        if (tipText != null)
            tipText.text = message;

        if (tipPanel != null)
            tipPanel.SetActive(true);

        yield return new WaitForSeconds(tipDisplayDuration);

        if (tipPanel != null)
            tipPanel.SetActive(false);

        tipCoroutine = null;
    }
}
