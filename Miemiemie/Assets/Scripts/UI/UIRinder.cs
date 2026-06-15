using UnityEngine;

public class UIRebinder : MonoBehaviour
{
    void Start()
    {
        // 如果你把 UI 引用设为 public，可以拖拽重新绑定
        // 或者通过 Find 来找

        // 简单方案：在场景加载后手动刷新
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RefreshAllUI();
        }
    }
}