using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class FixedRoomManager : MonoBehaviour
{
    public static FixedRoomManager Instance;

    [Header("当前地图")]
    [SerializeField] private FixedMapData currentMap;

    [Header("房间预制体")]
    [SerializeField] private GameObject emptyRoomPrefab;
    [SerializeField] private GameObject battleRoomPrefab;
    [SerializeField] private GameObject eventRoomPrefab;
    [SerializeField] private GameObject shopRoomPrefab;
    [SerializeField] private GameObject bossRoomPrefab;

    [Header("玩家")]
    [SerializeField] private GameObject playerPrefab;

    // 运行时数据
    private RoomConfig currentRoom;
    private GameObject currentRoomInstance;
    private GameObject playerInstance;
    private Dictionary<string, bool> clearedRooms = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化通关记录
        clearedRooms.Clear();
        foreach (var room in currentMap.rooms)
        {
            clearedRooms[room.roomId] = false;
            Debug.Log($"初始化房间: {room.roomId} = false");
        }

        // 创建玩家（只创建一次，永远不销毁）
        CreatePlayer();

        // 加载起始房间
        RoomConfig startRoom = currentMap.GetStartRoom();
        if (startRoom != null)
        {
            LoadRoom(startRoom);
        }
    }

    // 创建玩家（只执行一次）
    private void CreatePlayer()
    {
        playerInstance = Instantiate(playerPrefab);
        DontDestroyOnLoad(playerInstance);
        playerInstance.tag = "Player"; // 确保标签正确
    }

    // 加载房间
    public void LoadRoom(RoomConfig roomConfig)
    {
        StartCoroutine(TransitionToRoom(roomConfig));
    }

    private IEnumerator TransitionToRoom(RoomConfig newRoom)
    {
        // 清理旧房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        currentRoom = newRoom;

        // 创建新房间
        GameObject prefab = GetRoomPrefab(newRoom);
        currentRoomInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 设置房间
        RoomBase roomScript = currentRoomInstance.GetComponent<RoomBase>();
        if (roomScript != null)
        {
            roomScript.SetupRoom(newRoom);

            // 把玩家移动到新房间的出生点
            if (playerInstance != null && roomScript.playerSpawnPoint != null)
            {
                playerInstance.transform.position = roomScript.playerSpawnPoint.position;

                // 重置玩家状态（可选）
                ResetPlayerState();
            }
        }

        yield return null;
    }

    // 移动到目标房间
    public void MoveToRoom(string targetRoomId)
    {
        RoomConfig targetRoom = currentMap.GetRoomById(targetRoomId);
        if (targetRoom != null)
        {
            LoadRoom(targetRoom);
        }
    }

    // 标记房间已通关
    public void MarkRoomCleared(string roomId)
    {
        if (clearedRooms.ContainsKey(roomId))
        {
            clearedRooms[roomId] = true;
        }
    }

    // 检查房间是否已通关
    public bool IsRoomCleared(string roomId)
    {
        return clearedRooms.ContainsKey(roomId) && clearedRooms[roomId];
    }

    // 获取当前地图
    public FixedMapData GetCurrentMap()
    {
        return currentMap;
    }

    // 获取玩家
    public GameObject GetPlayer()
    {
        return playerInstance;
    }
    private void ResetPlayerState()
    {
        if (playerInstance == null) return;

        Health playerHealth = playerInstance.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
        }

        Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    private GameObject GetRoomPrefab(RoomConfig roomConfig)
    {
        if (roomConfig.customRoomPrefab != null)
            return roomConfig.customRoomPrefab;

        switch (roomConfig.roomType)
        {
            case RoomType.Empty: return emptyRoomPrefab;
            case RoomType.Battle: return battleRoomPrefab;
            case RoomType.Event: return eventRoomPrefab;
            case RoomType.Shop: return shopRoomPrefab;
            case RoomType.Boss: return bossRoomPrefab;
            default: return emptyRoomPrefab;
        }
    }

    // 返回家园
    public void ReturnToHome(bool victory)
    {
        // 战败处理：通知行动点管理器
        if (!victory && ActionPointManager.Instance != null)
        {
            ActionPointManager.Instance.DefeatedInHunt();
        }

        // ========== 清空所有对象池 ==========
        if (PoolManager.Instance != null)PoolManager.Instance.ClearAllPools();

        if (EffectPool.Instance != null)EffectPool.Instance.Clear();

        if (SporePool.Instance != null)SporePool.Instance.Clear();

        // 销毁玩家
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        // 销毁当前房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
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
    }
}