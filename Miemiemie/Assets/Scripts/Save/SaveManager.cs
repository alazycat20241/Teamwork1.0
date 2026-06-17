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

    [SerializeField] private AudioClip LoadSound;  // 在Inspector中拖入对应的音效

    private List<TechData> pendingTechData;

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

        // 恢复 UI（每次进家园都要）
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.FindUIElements();
            PlayerInventory.Instance.UpdateAllUI();
        }

        // 新游戏 → 重置科技树，其他什么都不做
        if (needResetTech)
        {
            ResetAllTechButtons();
            needResetTech = false;
            return;
        }

        // 从其他场景回来 → 恢复田块、科技树、玩偶
        if (isReturningFromOtherScene)
        {
            RestoreFarmBlocks();
            RestoreTech();

            if (DollPlay.Instance != null && PlayerInventory.Instance != null)
            {
                DollPlay.Instance.DCount = PlayerInventory.Instance.DollCount;
                DollPlay.Instance.SpawnDolls(PlayerInventory.Instance.DollCount);
            }

            isReturningFromOtherScene = false;
        }
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

        // ===== 新增：收集科技 =====
        pendingTechData = new List<TechData>();
        foreach (var u in FindObjectsOfType<Unlock>(true)) pendingTechData.Add(u.GetSaveData());
        foreach (var a in FindObjectsOfType<Attack>(true)) pendingTechData.Add(a.GetSaveData());
        foreach (var m in FindObjectsOfType<MaxHealth>(true)) pendingTechData.Add(m.GetSaveData());
        foreach (var r in FindObjectsOfType<Range>(true)) pendingTechData.Add(r.GetSaveData());
        foreach (var s in FindObjectsOfType<Speed>(true)) pendingTechData.Add(s.GetSaveData());
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

    void RestoreTech()
    {
        if (pendingTechData == null || pendingTechData.Count == 0) return;
        StartCoroutine(RestoreTechCoroutine());
    }

    IEnumerator RestoreTechCoroutine()
    {
        yield return null;
        var dict = new Dictionary<string, TechData>();
        foreach (var t in pendingTechData)
            if (!string.IsNullOrEmpty(t.techID)) dict[t.techID] = t;

        foreach (var u in FindObjectsOfType<Unlock>(true))
            if (dict.TryGetValue(u.TechID, out var d)) u.LoadFromSave(d.isUnlocked);

        foreach (var a in FindObjectsOfType<Attack>(true))
            if (dict.TryGetValue(a.TechID, out var d)) a.LoadFromSave(d.isPurchased);

        foreach (var m in FindObjectsOfType<MaxHealth>(true))
            if (dict.TryGetValue(m.TechID, out var d)) m.LoadFromSave(d.isPurchased);

        foreach (var r in FindObjectsOfType<Range>(true))
            if (dict.TryGetValue(r.TechID, out var d)) r.LoadFromSave(d.isPurchased);

        foreach (var s in FindObjectsOfType<Speed>(true))
            if (dict.TryGetValue(s.TechID, out var d)) s.LoadFromSave(d.isPurchased);

        pendingTechData = null;
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

        data.techDataList = new List<TechData>();
        foreach (var u in FindObjectsOfType<Unlock>(true)) data.techDataList.Add(u.GetSaveData());
        foreach (var a in FindObjectsOfType<Attack>(true)) data.techDataList.Add(a.GetSaveData());
        foreach (var m in FindObjectsOfType<MaxHealth>(true)) data.techDataList.Add(m.GetSaveData());
        foreach (var r in FindObjectsOfType<Range>(true)) data.techDataList.Add(r.GetSaveData());
        foreach (var s in FindObjectsOfType<Speed>(true)) data.techDataList.Add(s.GetSaveData());

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
        // ★ 播放音效
        if (LoadSound != null)
        {
            AudioManager.Instance.PlaySound(LoadSound);
        }

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
        pendingTechData = data.techDataList;
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

    private bool needResetTech = false;
    /// <summary>
    /// 新游戏时清除所有缓存
    /// </summary>
    public void ResetAllCache()
    {
        pendingFarmData = null;
        pendingTechData = null;
        isReturningFromOtherScene = false;
        needResetTech = true;   // 标记需要重置

        // 重置背包
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.ResetData();

        // 重置行动点
        if (ActionPointManager.Instance != null)
            ActionPointManager.Instance.ResetData();

        // 重置属性
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ResetData();
    }

    void ResetAllTechButtons()
    {
        StartCoroutine(ResetTechCoroutine());
    }

    IEnumerator ResetTechCoroutine()
    {
        yield return null;

        foreach (var u in FindObjectsOfType<Unlock>(true))
            u.ResetState();

        foreach (var a in FindObjectsOfType<Attack>(true))
            a.ResetState();

        foreach (var m in FindObjectsOfType<MaxHealth>(true))
            m.ResetState();

        foreach (var r in FindObjectsOfType<Range>(true))
            r.ResetState();

        foreach (var s in FindObjectsOfType<Speed>(true))
            s.ResetState();
    }
}