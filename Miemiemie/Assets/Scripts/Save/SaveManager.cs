using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SaveData;

/// <summary>
/// 存档管理器
/// 负责：
///   1. 存档到 5 个槽位（JSON 文件）
///   2. 从槽位读档
///   3. 场景切换时自动保存/恢复家园状态（田块、科技树、玩偶）
///   4. 新游戏时重置所有数据
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;          // 单例

    private const int MAX_SLOTS = 5;             // 最多存档槽位
    private string saveFolderPath;               // 存档文件夹的完整路径

    // ======================================================
    // 场景切换缓存（离开家园前存，回来后恢复）
    // ======================================================
    private List<GrowBlockData> pendingFarmData;  // 田块数据缓存
    private List<TechData> pendingTechData;       // 科技树数据缓存
    private bool isReturningFromOtherScene = false;  // 是否正在从其他场景返回家园
    private bool needResetTech = false;              // 新游戏是否需要重置科技树

    [SerializeField] private AudioClip LoadSound;    // 读档音效

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 跨场景保留

        // ===== 创建存档文件夹 =====
        // Application.persistentDataPath = 系统提供的永久存储目录
        saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");

        // 如果文件夹不存在，就创建一个
        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);

        // ===== 监听场景加载：每次加载 Home 时自动恢复状态 =====
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // 退订事件，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ======================================================
    // 场景加载完成时自动调用
    // ======================================================
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 只处理 Home 场景
        if (scene.name != "Home") return;

        // ===== 每次进家园，刷新 UI =====
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.FindUIElements();
            PlayerInventory.Instance.UpdateAllUI();
        }

        // ===== 新游戏 → 重置科技树按钮 =====
        if (needResetTech)
        {
            isReturningFromOtherScene = false;
            ResetAllTechButtons();
            needResetTech = false;
            return;
        }

        // ===== 从其他场景回来 → 恢复田块、科技、玩偶 =====
        if (isReturningFromOtherScene)
        {
            RestoreFarmBlocks();    // 恢复田块状态
            RestoreTech();          // 恢复科技树状态

            // 恢复玩偶
            if (DollPlay.Instance != null && PlayerInventory.Instance != null)
            {
                DollPlay.Instance.DCount = PlayerInventory.Instance.DollCount;
                DollPlay.Instance.SpawnDolls(PlayerInventory.Instance.DollCount);
            }

            isReturningFromOtherScene = false;
        }
    }

    // ======================================================
    // 离开家园前调用：缓存当前家园状态
    // ======================================================
    public void SaveFarmBeforeSceneChange()
    {
        isReturningFromOtherScene = true;

        // ===== 收集所有田块的状态 =====
        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();
        pendingFarmData = new List<GrowBlockData>();
        foreach (var block in allBlocks)
        {
            if (block != null)
                pendingFarmData.Add(block.GetSaveData());  // 每个田块自己打包数据
        }

        // ===== 收集所有科技按钮的状态 =====
        pendingTechData = new List<TechData>();
        foreach (var u in FindObjectsOfType<Unlock>(true)) pendingTechData.Add(u.GetSaveData());
        foreach (var a in FindObjectsOfType<Attack>(true)) pendingTechData.Add(a.GetSaveData());
        foreach (var m in FindObjectsOfType<MaxHealth>(true)) pendingTechData.Add(m.GetSaveData());
        foreach (var r in FindObjectsOfType<Range>(true)) pendingTechData.Add(r.GetSaveData());
        foreach (var s in FindObjectsOfType<Speed>(true)) pendingTechData.Add(s.GetSaveData());
    }

    // ======================================================
    // 恢复田块状态（回到家园时调用）
    // ======================================================
    public void RestoreFarmBlocks()
    {
        if (pendingFarmData == null || pendingFarmData.Count == 0) return;
        StartCoroutine(RestoreFarmCoroutine());
    }

    IEnumerator RestoreFarmCoroutine()
    {
        // 等待一帧，确保场景里的田块已经实例化完成
        yield return null;

        if (pendingFarmData == null || pendingFarmData.Count == 0) yield break;

        // 找到当前场景里所有田块
        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();
        if (allBlocks == null || allBlocks.Length == 0)
        {
            pendingFarmData = null;
            yield break;
        }

        // 建立字典：blockID → 田块对象（方便快速查找）
        Dictionary<string, GrowBlock> blockDict = new Dictionary<string, GrowBlock>();
        foreach (var block in allBlocks)
        {
            if (block == null) continue;
            if (string.IsNullOrEmpty(block.blockID)) continue;
            if (blockDict.ContainsKey(block.blockID)) continue;  // 跳过重复 ID

            blockDict[block.blockID] = block;
        }

        // 根据缓存数据恢复每个田块
        int restored = 0;
        for (int i = 0; i < pendingFarmData.Count; i++)
        {
            var blockData = pendingFarmData[i];
            if (blockData == null) continue;
            if (string.IsNullOrEmpty(blockData.blockID)) continue;

            // 找到对应的田块对象
            if (blockDict.TryGetValue(blockData.blockID, out GrowBlock block))
            {
                block.LoadFromSaveData(blockData);  // 恢复状态
                restored++;
            }
        }

        // 清空缓存
        pendingFarmData = null;
    }

    // ======================================================
    // 恢复科技树状态（回到家园时调用）
    // ======================================================
    void RestoreTech()
    {
        if (pendingTechData == null || pendingTechData.Count == 0) return;
        StartCoroutine(RestoreTechCoroutine());
    }

    IEnumerator RestoreTechCoroutine()
    {
        yield return null;

        // 建立字典：techID → 科技数据
        var dict = new Dictionary<string, TechData>();
        foreach (var t in pendingTechData)
            if (!string.IsNullOrEmpty(t.techID)) dict[t.techID] = t;

        // 恢复各种科技按钮的状态
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
    // 根据槽位号拼出完整文件路径
    // 例如：GetSlotPath(0) → "C:/.../Saves/save_0.json"
    // ======================================================
    string GetSlotPath(int index) => Path.Combine(saveFolderPath, $"save_{index}.json");

    // ======================================================
    // 存档到指定槽位
    // ======================================================
    public bool SaveToSlot(int index)
    {
        // 槽位号不合法 → 失败
        if (index < 0 || index >= MAX_SLOTS) return false;

        // 创建存档数据对象
        SaveData data = new SaveData();

        // --- 收集背包数据 ---
        if (PlayerInventory.Instance != null)
        {
            data.seedCount = PlayerInventory.Instance.seedCount;
            data.playerGold = PlayerInventory.Instance.playerGold;
            data.soulStones = PlayerInventory.Instance.soulStones;
            data.dollCount = PlayerInventory.Instance.DollCount;
        }

        // --- 收集行动点数据 ---
        if (ActionPointManager.Instance != null)
        {
            data.currentActionPoints = ActionPointManager.Instance.GetCurrentPoints();
            data.currentDay = ActionPointManager.Instance.GetCurrentDay();
            data.maxActionPoints = ActionPointManager.Instance.maxActionPoints;
        }

        // --- 收集所有田块数据 ---
        GrowBlock[] allBlocks = FindObjectsOfType<GrowBlock>();
        data.growBlockDataList = new List<GrowBlockData>();
        foreach (var block in allBlocks)
        {
            if (block != null)
                data.growBlockDataList.Add(block.GetSaveData());
        }

        // --- 收集所有科技树数据 ---
        data.techDataList = new List<TechData>();
        foreach (var u in FindObjectsOfType<Unlock>(true)) data.techDataList.Add(u.GetSaveData());
        foreach (var a in FindObjectsOfType<Attack>(true)) data.techDataList.Add(a.GetSaveData());
        foreach (var m in FindObjectsOfType<MaxHealth>(true)) data.techDataList.Add(m.GetSaveData());
        foreach (var r in FindObjectsOfType<Range>(true)) data.techDataList.Add(r.GetSaveData());
        foreach (var s in FindObjectsOfType<Speed>(true)) data.techDataList.Add(s.GetSaveData());

        // --- 记录存档时间 ---
        data.saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm");

        // --- 转成 JSON 写入文件 ---
        string json = JsonUtility.ToJson(data, true);   // true = 格式化（可读性更好）
        File.WriteAllText(GetSlotPath(index), json);     // 写入对应槽位文件

        return true;
    }

    // ======================================================
    // 从指定槽位读档
    // ======================================================
    public bool LoadFromSlot(int index)
    {
        // 播放读档音效
        if (LoadSound != null)
            AudioManager.Instance.PlaySound(LoadSound);

        // 恢复时间流速
        Time.timeScale = 1f;

        // 检查存档文件是否存在
        string path = GetSlotPath(index);
        if (!File.Exists(path)) return false;

        // --- 读取 JSON 并解析 ---
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // --- 恢复背包数据 ---
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.seedCount = data.seedCount;
            PlayerInventory.Instance.playerGold = data.playerGold;
            PlayerInventory.Instance.soulStones = data.soulStones;
            PlayerInventory.Instance.DollCount = data.dollCount;
        }

        // --- 恢复行动点数据 ---
        if (ActionPointManager.Instance != null)
        {
            ActionPointManager.Instance.LoadData(
                data.currentActionPoints,
                data.currentDay,
                data.maxActionPoints
            );
        }

        // --- 缓存田块和科技数据（等场景加载后恢复）---
        pendingFarmData = data.growBlockDataList;
        pendingTechData = data.techDataList;
        isReturningFromOtherScene = true;

        // --- 加载家园场景 ---
        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene("Home");
        else
            SceneManager.LoadScene("Home");

        return true;
    }

    // ======================================================
    // 工具方法
    // ======================================================

    /// <summary>检查某个槽位是否有存档</summary>
    public bool SlotHasSave(int index) => File.Exists(GetSlotPath(index));

    /// <summary>获取某个槽位的存档信息（用于显示存档时间等）</summary>
    public SaveData GetSlotInfo(int index)
    {
        string path = GetSlotPath(index);
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>删除某个槽位的存档</summary>
    public void DeleteSlot(int index)
    {
        string path = GetSlotPath(index);
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// 新游戏：重置所有缓存和数据
    /// </summary>
    public void ResetAllCache()
    {
        // 清空场景切换缓存
        pendingFarmData = null;
        pendingTechData = null;
        isReturningFromOtherScene = false;
        needResetTech = true;  // 标记需要重置科技树

        // 重置背包
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.ResetData();

        // 重置行动点
        if (ActionPointManager.Instance != null)
            ActionPointManager.Instance.ResetData();

        // 重置玩家属性
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ResetData();
    }

    /// <summary>
    /// 清理存档缓存（返回菜单时调用，不重置玩家数据）
    /// </summary>
    public void ClearPendingData()
    {
        pendingFarmData = null;
        pendingTechData = null;
        isReturningFromOtherScene = false;
        needResetTech = false;
    }

    /// <summary>重置所有科技树按钮到初始状态</summary>
    void ResetAllTechButtons()
    {
        StartCoroutine(ResetTechCoroutine());
    }

    IEnumerator ResetTechCoroutine()
    {
        // 等一帧确保科技按钮已实例化
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