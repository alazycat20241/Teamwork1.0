using UnityEngine;

public class HomeSceneInit : MonoBehaviour
{
    void Start()
    {
        // 恢复 UI
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.FindUIElements();
            PlayerInventory.Instance.UpdateAllUI();
        }

        // 恢复玩偶
        if (DollPlay.Instance != null && PlayerInventory.Instance != null)
        {
            DollPlay.Instance.SpawnDolls(PlayerInventory.Instance.DollCount);
        }

        // 恢复田块
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RestoreFarmBlocks();
        }
    }
}