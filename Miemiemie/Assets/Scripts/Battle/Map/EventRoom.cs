using UnityEngine;

public class EventRoom : RoomBase
{
    public override void SetupRoom(RoomConfig config)
    {
        base.SetupRoom(config);

        // 直接激活出口，不打怪
        OnRoomCompleted();
    }
}
