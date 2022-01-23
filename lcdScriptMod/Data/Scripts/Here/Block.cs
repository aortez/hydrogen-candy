using System;
using System.Collections.Generic;
using System.Text;
﻿using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

using Digi;

using IMyCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyEntity = VRage.Game.ModAPI.Ingame.IMyEntity;
using IMyInventory = VRage.Game.ModAPI.Ingame.IMyInventory;
using IMySlimBlock = VRage.Game.ModAPI.IMySlimBlock;

namespace AllanTTS
{

// Custom block "AllanLargeLCDPanel".  What does it do?  Not sure yet...
[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false)]
public class ProtectionBlock : MyGameLogicComponent
{
    public bool IsOptionSet;

    private IMyFunctionalBlock block;

    private static bool createdTerminalControls = false;

    // this method is called async! always do stuff in the first update unless you're sure it must be in Init().
    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
    }

    public override void UpdateOnceBeforeFrame() // first update of the block
    {
        block = (IMyFunctionalBlock)Entity;

        if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
            return;

        TssSession.Instance?.ProtectionBlocks.Add(block);

        // CreateTerminalControls();
    }

    public override void Close() // called when block is removed for whatever reason (including ship despawn)
    {
        TssSession.Instance?.ProtectionBlocks.Remove((IMyFunctionalBlock)Entity);
    }

    public override void UpdateBeforeSimulation()
    {
        if (!block.IsFunctional)
        {
            return;
        }

        Log.Info("UpdateBeforeSimulation", "UpdateBeforeSimulation", 500);
    }

}

}
