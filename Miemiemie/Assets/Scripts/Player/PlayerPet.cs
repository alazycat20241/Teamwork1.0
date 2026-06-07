using UnityEngine;

public class PlayerPet : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float smoothTime = 0.3f;

    [Header("骨骼翻转")]
    [SerializeField] private Transform rootBone;
    [SerializeField] private bool facePlayer = true;

    [Header("浮动")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private Transform floatTarget;  // ★ 用于浮动的额外物体

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
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime,
                followSpeed
            );
        }
        else
        {
            velocity = Vector3.zero;
        }

        // 翻转（翻转 floatTarget 而不是 rootBone）
        if (facePlayer && floatTarget != null)
        {
            bool shouldFlip = player.position.x < transform.position.x;
            Vector3 scale = floatTarget.localScale;
            scale.x = shouldFlip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            floatTarget.localScale = scale;
        }

        // 浮动应用到 floatTarget 的本地Y（动画改 rootBone，互不干扰）
        if (enableFloat && floatTarget != null)
        {
            float floatValue = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            Vector3 pos = floatTarget.localPosition;
            pos.y = floatValue;
            floatTarget.localPosition = pos;
        }
    }

    public void TeleportToPlayer()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
            velocity = Vector3.zero;
        }
    }
}