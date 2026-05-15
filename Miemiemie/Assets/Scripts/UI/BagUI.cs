using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BagUI : MonoBehaviour
{
    public static BagUI Instance { get; private set; }

    [Header("背包面板")]
    public SlidePanel bagPanel;

    [Header("UI引用")]
    public Transform contentParent;
    public GameObject bagItemPrefab;

    [Header("种子数据")]
    public ItemData seedItem;   // 在 Inspector 里拖一个种子 ItemData

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        RefreshBag();
    }

    public void OpenBag()
    {
        bagPanel.Open();
    }

    public void CloseBag()
    {
        bagPanel.Close();
    }

    public void RefreshBag()
    {
        foreach (Transform t in contentParent)
            Destroy(t.gameObject);

        int count = PlayerInventory.Instance.seedCount;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(bagItemPrefab, contentParent);
            SetupBagItem(go, seedItem);
        }
    }

    private void SetupBagItem(GameObject go, ItemData item)
    {
        go.transform.Find("Image").GetComponent<Image>().sprite = item.icon;
        go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.itemName;
    }
}