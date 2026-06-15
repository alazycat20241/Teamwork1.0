using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SaveData;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const int MAX_SLOTS = 5;
    private string saveFolderPath;

    // 田块缓存（场景切换前保存，回来后恢复）
    private List<GrowBlockData> pendingFarmData;
    private bool isReturningFromOtherScene = false;

    void Awake()
    {
        // ===== 单例 =====
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ===== 创建存档文件夹 =====
        saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);

        // ===== 监听场景加载 =====
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ======================================================
    // 场景加载完成时自动调用
    // ======================================================
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Home") return;

        // 恢复 UI
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.FindUIElements();
            PlayerInventory.Instance.UpdateAllUI();
        }

        if (!isReturningFromOtherScene)
        {
            Debug.Log("[SaveManager] 第一次进家园，不恢复");
            return;
        }

        Debug.Log("[SaveManager] 开始恢复田块和玩偶");
        RestoreFarmBlocks();

        if (DollPlay.Instance != null && PlayerInventory.Instance != null)
        {
            DollPlay.Instance.DCount = PlayerInventory.Instance.DollCount;
            DollPlay.Instance.SpawnDolls(PlayerInventory.Instance.DollCount);
        }

        isReturningFromOtherScene = false;
    }

    // ======================================================
    // 切换场景前调用（在进入战斗等场景之前）
    // ======================================================
    public void SaveFarmBeforeSceneChange()
    {
        isReturningFromOtherScene = true;

        // 收集当前所有田块状态
        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();
        pendingFarmData = new List<GrowBlockData>();
        foreach (var block in allBlocks)
        {
            if (block != null)
                pendingFarmData.Add(block.GetSaveData());
        }
    }

    // ======================================================
    // 恢复田块状态
    // ======================================================
    public void RestoreFarmBlocks()
    {
        if (pendingFarmData == null || pendingFarmData.Count == 0) return;

        StartCoroutine(RestoreFarmCoroutine());
    }

    IEnumerator RestoreFarmCoroutine()
    {
        yield return null;

        if (pendingFarmData == null || pendingFarmData.Count == 0)
        {
            yield break;
        }

        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();

        if (allBlocks == null || allBlocks.Length == 0)
        {
            pendingFarmData = null;
            yield break;
        }

        // 建立字典
        Dictionary<string, GrowBlock> blockDict = new Dictionary<string, GrowBlock>();
        foreach (var block in allBlocks)
        {
            if (block == null) continue;

            if (string.IsNullOrEmpty(block.blockID))
            {
                continue;
            }

            if (blockDict.ContainsKey(block.blockID))
            {
                continue;
            }

            blockDict[block.blockID] = block;
        }

        // 恢复
        int restored = 0;
        for (int i = 0; i < pendingFarmData.Count; i++)
        {
            var blockData = pendingFarmData[i];
            if (blockData == null) continue;

            if (string.IsNullOrEmpty(blockData.blockID))
            {
                continue;
            }

            if (blockDict.TryGetValue(blockData.blockID, out GrowBlock block))
            {
                block.LoadFromSaveData(blockData);
                restored++;
            }
        }
        pendingFarmData = null;
    }

    // ======================================================
    // 存档到槽位
    // ======================================================
    string GetSlotPath(int index) => Path.Combine(saveFolderPath, $"save_{index}.json");

    public bool SaveToSlot(int index)
    {
        if (index < 0 || index >= MAX_SLOTS) return false;

        SaveData data = new SaveData();

        // 背包数据
        if (PlayerInventory.Instance != null)
        {
            data.seedCount = PlayerInventory.Instance.seedCount;
            data.playerGold = PlayerInventory.Instance.playerGold;
            data.soulStones = PlayerInventory.Instance.soulStones;
            data.dollCount = PlayerInventory.Instance.DollCount;
        }

        // 行动点数据
        if (ActionPointManager.Instance != null)
        {
            data.currentActionPoints = ActionPointManager.Instance.GetCurrentPoints();
            data.currentDay = ActionPointManager.Instance.GetCurrentDay();
            data.maxActionPoints = ActionPointManager.Instance.maxActionPoints;
        }

        // 田块数据
        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();

        data.growBlockDataList = new List<GrowBlockData>();
        foreach (var block in allBlocks)
        {
            if (block != null)
            {
                var blockData = block.GetSaveData();
                data.growBlockDataList.Add(blockData);
            }
        }

        // 时间戳
        data.saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm");

        // 写入文件
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(index), json);

        return true;
    }

    // ======================================================
    // 从槽位读档
    // ======================================================
    public bool LoadFromSlot(int index)
    {
        Time.timeScale = 1f;

        string path = GetSlotPath(index);
        if (!File.Exists(path)) return false;

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        // 恢复背包数据
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.seedCount = data.seedCount;
            PlayerInventory.Instance.playerGold = data.playerGold;
            PlayerInventory.Instance.soulStones = data.soulStones;
            PlayerInventory.Instance.DollCount = data.dollCount;
        }

        // 恢复行动点数据
        if (ActionPointManager.Instance != null)
        {
            ActionPointManager.Instance.LoadData(
                data.currentActionPoints,
                data.currentDay,
                data.maxActionPoints
            );
        }

        // 缓存田块数据，等场景加载后恢复
        pendingFarmData = data.growBlockDataList;
        isReturningFromOtherScene = true;

        // 加载家园场景
        if (SceneTransition.Instance != null)
        {
            //SaveFarmBeforeSceneChange();
            SceneTransition.Instance.LoadScene("Home");
        }
        else
        {
            SceneManager.LoadScene("Home");
        }

        return true;
    }

    // ======================================================
    // 工具方法
    // ======================================================
    public bool SlotHasSave(int index) => File.Exists(GetSlotPath(index));

    public SaveData GetSlotInfo(int index)
    {
        string path = GetSlotPath(index);
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void DeleteSlot(int index)
    {
        string path = GetSlotPath(index);
        if (File.Exists(path)) File.Delete(path);
    }
}