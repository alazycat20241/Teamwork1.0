using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopRoom : RoomBase
{
    public override void SetupRoom(RoomConfig config)
    {
        base.SetupRoom(config);

        // 直接激活出口，不打怪
        OnRoomCompleted();
    }
}
