using UnityEngine;

public class PlayerPet : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float minDistance = 0.5f;//距离玩家的最近距离
    [SerializeField] private float smoothTime = 0.3f;// 平滑时间（越小越灵敏，越大越迟缓）

    [Header("骨骼翻转")]
    [SerializeField] private Transform rootBone;//骨骼根节点
    [SerializeField] private bool facePlayer = true;

    [Header("浮动")]
    [SerializeField] private bool enableFloat = true;//是否开始浮动
    [SerializeField] private float floatAmplitude = 0.15f;// 浮动幅度（飘多高）
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private Transform floatTarget;  // ★ 用于浮动的额外物体（单独一个空物体，夹在骨骼和本体之间）

    private Transform player;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;
    private float baseY;

    Transform GetPlayer()
    {
        if (FixedRoomManager.Instance != null)
        {
            GameObject obj = FixedRoomManager.Instance.GetPlayer();
            if (obj != null) return obj.transform;
        }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) return playerObj.transform;
        return null;
    }

    void Start()
    {
        player = GetPlayer();
        if (rootBone == null && transform.childCount > 0)
            rootBone = transform.GetChild(0);

        if (player != null)
            offset = transform.position - player.position;

        //动画会修改 rootBone 的本地坐标（走路晃动等）
        // 浮动也会修改本地坐标（上下飘）
        // 如果都改 rootBone → 互相打架
        // 解决：在 rootBone 上面加一层 FloatPivot（空物体）
        //   FloatPivot → 负责浮动（上下飘）
        //   └── rootBone → 负责动画（走路晃动）

        // 如果没指定浮动目标，创建一个空物体夹在中间
        if (floatTarget == null && rootBone != null)
        {
            GameObject floatObj = new GameObject("FloatPivot");
            floatObj.transform.SetParent(transform);
            floatObj.transform.localPosition = Vector3.zero;
            rootBone.SetParent(floatObj.transform);
            floatTarget = floatObj.transform;
        }

        baseY = transform.position.y;
    }

    void Update()
    {
        if (player == null)
        {
            player = GetPlayer();
            return;
        }

        Vector3 targetPosition = player.position + offset;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > minDistance)
        {
            // SmoothDamp：平滑移动，开始快结束慢，不会突然加速
            transform.position = Vector3.SmoothDamp(
                transform.position,   // 从当前位置
                targetPosition,       // 去目标位置
                ref velocity,         // 速度缓冲，&
                smoothTime,           // 平滑时间（0.3 秒接近目标）
                followSpeed           // 最大速度
            );
        }
        else
        {
            velocity = Vector3.zero;
        }

        // 翻转（翻转 floatTarget 而不是 rootBone）
        if (facePlayer && floatTarget != null)
        {
            // 玩家在跟宠左边 → shouldFlip = true → 面向左边
            // 玩家在跟宠右边 → shouldFlip = false → 面向右边
            bool shouldFlip = player.position.x < transform.position.x;
            Vector3 scale = floatTarget.localScale;
            scale.x = shouldFlip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            floatTarget.localScale = scale;
        }

        // 浮动应用到 floatTarget 的本地Y（动画改 rootBone，互不干扰）
        if (enableFloat && floatTarget != null)
        {// Time.time：游戏运行的总秒数（持续增长）
            float floatValue = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

            Vector3 pos = floatTarget.localPosition;
            pos.y = floatValue;
            floatTarget.localPosition = pos;
        }
    }


    /// <summary>
    /// 瞬间传送到玩家身边（切换房间时调用）
    /// </summary>
    public void TeleportToPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
            velocity = Vector3.zero;
        }
    }
}