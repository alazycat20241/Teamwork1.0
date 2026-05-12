using UnityEngine;

[CreateAssetMenu(menuName = "Create LaserAsset")]
public class LaserConfig : ScriptableObject
{
    public int ID = 0;                    // 对象池唯一ID
    public float maxLength = 10f;
    public float damagePerSecond = 30f;
    public float lifeTime = 0.5f;         // 有效伤害时间（之后开始消失动画）
    public float fadeOutDuration = 0.15f; // 消失动画时长
    public LayerMask targetLayer;         // 伤害目标层（Enemy）
    public LayerMask obstacleLayer;       // 墙壁等阻挡层
    public GameObject prefab;             // 激光预制体（带LaserBeam脚本）
}
