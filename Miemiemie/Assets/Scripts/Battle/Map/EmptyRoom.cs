using UnityEngine;
public class EmptyRoom : RoomBase
{
    public override void SetupRoom(RoomConfig config)//重写setuproom
    {
        base.SetupRoom(config);

        // 空房间直接激活出口
        OnRoomCompleted();
    }
}