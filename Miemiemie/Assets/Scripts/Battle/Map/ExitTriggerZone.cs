using UnityEngine;

public class ExitTriggerZone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;

    private RoomExit exitData;
    private Collider2D triggerCollider;
    private bool isActive = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.enabled = false;   // 默认碰撞关闭
    }

    public void Setup(RoomExit exit)
    {
        exitData = exit;
    }

    public void Activate()
    {
        isActive = true;
        triggerCollider.enabled = true;
        Debug.Log("TRUE");

    }

    public void Deactivate()
    {
        isActive = false;
        triggerCollider.enabled = false;
        Debug.Log("FALSE");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
        {
            Debug.Log("碰撞了但 isActive 是 false");
            return;
        }

        if (exitData == null)
        {
            Debug.LogError($"exitData 是 null！物体名: {gameObject.name}");
            return;
        }

        if (FixedRoomManager.Instance == null)
        {
            Debug.LogError("FixedRoomManager.Instance 是 null！");
            return;
        }

        if (other.CompareTag("Player"))
        {
            FixedRoomManager.Instance.MoveToRoom(exitData.targetRoomId);
        }
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            FixedRoomManager.Instance.MoveToRoom(exitData.targetRoomId);
        }
    }
}