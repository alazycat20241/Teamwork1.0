using UnityEngine;
public class EmptyRoom : RoomBase
{
    public override void SetupRoom(RoomConfig config)
    {
        base.SetupRoom(config);

        // 空房间直接激活出口，不需要打怪
        OnRoomCompleted();
    }
}