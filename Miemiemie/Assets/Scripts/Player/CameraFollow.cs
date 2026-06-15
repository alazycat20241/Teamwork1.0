using UnityEngine;

/// <summary>
/// 摄像头固定框住整个房间，不跟随玩家
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10);

    private Vector3 targetPosition;
    private bool moving = false;

    void Start()
    {
        targetPosition = transform.position;
    }

    /// <summary>
    /// 切换到新房间（由 FixedRoomManager 调用）
    /// </summary>
    public void MoveToRoom(Vector3 roomPosition)
    {
        targetPosition = roomPosition + offset;
        moving = true;
    }

    void LateUpdate()
    {
        if (!moving) return;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            transform.position = targetPosition;
            moving = false;
        }
    }
}