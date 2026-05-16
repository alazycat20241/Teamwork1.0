using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图节点
/// 挂载在每个节点按钮上，负责显示图标和响应点击
/// </summary>
public class MapNode : MonoBehaviour
{
    [Header("节点数据")]
    public int value;               // 节点唯一编号（自动分配）
    public int level;               // 所在层数（自动分配）
    public bool isUsed = false;     // 是否已激活（解锁可见）
    public bool isVisited = false;  // 是否已被玩家选择过
    public bool isReachable = false;// 当前是否可以点击
    public NodeType nodeType;       // 节点类型：战斗/事件/商店/Boss/起点

    [Header("连接关系（预留，当前用路径列表画线）")]
    public MapNode left;            // 左连接（预留）
    public MapNode right;           // 右连接（预留）

    [Header("各类型图标")]
    [SerializeField] private Sprite battleSprite;   // 战斗图标
    [SerializeField] private Sprite eventSprite;    // 事件图标
    [SerializeField] private Sprite shopSprite;     // 商店图标
    [SerializeField] private Sprite bossSprite;     // Boss图标

    [Header("UI组件")]
    [SerializeField] private Image nodeImage;       // 显示图标的Image
    [SerializeField] private Button button;         // 按钮组件

    private void Awake()
    {
        // 如果没有手动拖拽赋值，就自己GetComponent找
        if (button == null) button = GetComponent<Button>();

        // 绑定点击事件
        button.onClick.AddListener(OnClickNode);
    }

    /// <summary>
    /// 初始化节点状态（生成时调用一次）
    /// </summary>
    public void Init()
    {
        isUsed = false;
        isVisited = false;
        isReachable = false;
        gameObject.SetActive(false);     // 初始隐藏
        SetNodeTypeIcon();               // 根据类型设置图标
    }

    /// <summary>
    /// 激活节点：显示出来并且可以点击
    /// </summary>
    public void Activate()
    {
        isUsed = true;
        gameObject.SetActive(true);
        SetReachable(true);
    }

    /// <summary>
    /// 标记为已选择：变暗、不能再点
    /// </summary>
    public void MarkAsVisited()
    {
        isVisited = true;
        isReachable = false;
        button.interactable = false;                     // 按钮不可交互
    }

    /// <summary>
    /// 按钮点击回调
    /// </summary>
    private void OnClickNode()
    {
        // 只有可点击且没选过的节点才会响应
        if (isReachable && !isVisited)
        {
            // 通知BattleMap处理
            BattleMap.Instance.OnNodeClicked(this);
        }
    }

    /// <summary>
    /// 设置节点是否可点击（影响按钮交互状态和颜色）
    /// </summary>
    /// <param name="reachable">true=可以点，false=灰色不可点</param>
    public void SetReachable(bool reachable)
    {
        isReachable = reachable;

        // 只有没选过的节点才需要更新按钮状态
        button.interactable = reachable && !isVisited;

        if (!isVisited)
        {
            // 可点击=白色，不可点击=半灰
            nodeImage.color = reachable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }

    /// <summary>
    /// 根据nodeType设置对应的图标
    /// </summary>
    public void SetNodeTypeIcon()
    {
        if (nodeImage == null) return;

        switch (nodeType)
        {
            case NodeType.Battle: nodeImage.sprite = battleSprite; break;
            case NodeType.Event: nodeImage.sprite = eventSprite; break;
            case NodeType.Shop: nodeImage.sprite = shopSprite; break;
            case NodeType.Boss: nodeImage.sprite = bossSprite; break;
        }
    }

    /// <summary>
    /// 重置节点到初始状态
    /// </summary>
    public void ResetNode()
    {
        isVisited = false;
        isReachable = false;
        isUsed = false;
        button.interactable = false;
        nodeImage.color = Color.white;   // 恢复白色
        gameObject.SetActive(false);     // 隐藏
    }

    /// <summary>
    /// 隐藏节点（未被选择的那个直接消失）
    /// </summary>
    public void Hide()
    {
        isUsed = false;
        isReachable = false;
        button.interactable = false;
        gameObject.SetActive(false);
    }
}