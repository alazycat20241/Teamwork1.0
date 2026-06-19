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
    }

    public void Deactivate()
    {
        isActive = false;
        triggerCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
        {
            return;
        }

        if (exitData == null)
        {
            return;
        }

        if (FixedRoomManager.Instance == null)
        {
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