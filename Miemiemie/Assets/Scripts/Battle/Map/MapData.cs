using System.Collections.Generic;
using UnityEngine;

// 地图数据 
[CreateAssetMenu(fileName = "NewMap", menuName = "Game/Fixed Map Data")]
public class FixedMapData : ScriptableObject
{
    [Header("地图名称")]
    public string mapName = "第一章";

    [Header("房间列表")]
    public List<RoomConfig> rooms = new List<RoomConfig>();

    // 获取起始房间
    public RoomConfig GetStartRoom()
    {
        return rooms.Find(r => r.roomType == RoomType.Empty);
    }

    // 根据ID获取房间
    public RoomConfig GetRoomById(string id)
    {
        return rooms.Find(r => r.roomId == id);
    }
}