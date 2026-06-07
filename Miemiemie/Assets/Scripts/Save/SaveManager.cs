using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const int MAX_SLOTS = 5;          // 最多5个存档槽
    private string saveFolderPath;            // 存档文件夹路径

    void Awake()
    {
        // 单例，场景切换不销毁
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);        // 移到根层级，避免DontDestroyOnLoad报错
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 创建存档文件夹
        saveFolderPath = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolderPath))
            Directory.CreateDirectory(saveFolderPath);

        Debug.Log("存档路径：" + saveFolderPath);
    }

    // 获取槽位文件路径
    string GetSlotPath(int index) => Path.Combine(saveFolderPath, $"save_{index}.json");

    // 保存到指定槽位
    public bool SaveToSlot(int index)
    {
        if (index < 0 || index >= MAX_SLOTS) return false;

        SaveData data = new SaveData();

        // 收集背包数据
        if (PlayerInventory.Instance != null)
        {
            data.seedCount = PlayerInventory.Instance.seedCount;
            data.playerGold = PlayerInventory.Instance.playerGold;
            data.soulStones = PlayerInventory.Instance.soulStones;
        }

        // 收集行动点数据
        if (ActionPointManager.Instance != null)
        {
            data.currentActionPoints = ActionPointManager.Instance.GetCurrentPoints();
            data.currentDay = ActionPointManager.Instance.GetCurrentDay();
            data.maxActionPoints = ActionPointManager.Instance.maxActionPoints;
        }

        // 记录存档时间
        data.saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm");

        // 写入文件
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(index), json);
        return true;
    }

    // 从指定槽位读档
    public bool LoadFromSlot(int index)
    {
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

        // 过渡加载
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("Home");
        }
        else
        {
            SceneManager.LoadScene("Home");  // 降级方案
        }

        return true;
    }

    // 检查槽位是否有存档
    public bool SlotHasSave(int index) => File.Exists(GetSlotPath(index));

    // 获取槽位存档信息（用于UI显示）
    public SaveData GetSlotInfo(int index)
    {
        string path = GetSlotPath(index);
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    // 删除槽位存档
    public void DeleteSlot(int index)
    {
        string path = GetSlotPath(index);
        if (File.Exists(path)) File.Delete(path);
    }
}