using UnityEngine;

[CreateAssetMenu(fileName = "NewProp", menuName = "Game/Prop Data")]
public class PropData : ScriptableObject
{
    [Header("基本信息")]
    [Tooltip("道具名称")]
    public string propName;

    [Tooltip("道具图标")]
    public Sprite icon;

    [Tooltip("道具描述文本")]
    [TextArea(2, 5)]
    public string description;

    [Tooltip("道具唯一ID（1-12）")]
    public int propID;
}