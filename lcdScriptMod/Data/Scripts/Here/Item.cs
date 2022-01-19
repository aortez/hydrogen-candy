using System.Collections.Generic;
using System;

using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

namespace AllanTTS
{

class Item {
    public Item(String displayName, String subTypeId, String typeId) {
        Count = 0;
        DisplayName = displayName;
        SubTypeId = subTypeId;
        Type = new MyItemType(typeId, subTypeId);
        TypeId = typeId;
    }

    public long Count;
    public String DisplayName;
    public String SubTypeId;
    public MyItemType Type;
    public String TypeId;
}

// Mode specified for this script.
// This controls what is processed/displayed.
public enum ItemGroups
{
    Ingots,
    IngotsAndOres,
    Ores,
}

}
