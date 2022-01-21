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

// Must use exact type, no inheritence.
[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "AllanLargeLCDPanel")]
public class ProtectionBlock : MyGameLogicComponent
{
    public bool IsOptionSet;

    private IMyFunctionalBlock block;

    private static bool createdTerminalControls = false;

    // this method is called async! always do stuff in the first update unless you're sure it must be in Init().
    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        Log.Info("ProtectionBlock.Init", "ProtectionBlock.Init", 500);
    }

    public override void UpdateOnceBeforeFrame() // first update of the block
    {
        block = (IMyFunctionalBlock)Entity;

        if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
            return;

        TssSession.Instance?.ProtectionBlocks.Add(block);

        CreateTerminalControls();

        Log.Info("UpdateOnceBeforeFrame", "UpdateOnceBeforeFrame", 500);
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

    private static void TerminalControls_CustomControlGetter(IMyTerminalBlock b, List<IMyTerminalControl> controls)
    {
        Log.Info("\nCandidate DefinitionDisplayNameText: " + b.DefinitionDisplayNameText);
        Log.Info("Candidate DetailedInfo: " + b.DetailedInfo);
        Log.Info("Candidate DisplayName: " + b.DisplayName);
        Log.Info("Candidate EntityId: " + b.EntityId);
        Log.Info("Candidate Name: " + b.Name);

        // if (!(b is IMyTextPanel)) {
        if (b.DefinitionDisplayNameText != "LCD Panel Large (Allan)") {
            return;
        }

        try {
            // Add a checkbox.  It doesn't do anything yet.
            var checkbox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyTextPanel>("DO somethign");
            checkbox.Title = MyStringId.GetOrCompute("Yeet");
            checkbox.Tooltip = MyStringId.GetOrCompute("You guessed it.\nNope, actually try again.");
            checkbox.Getter = (block) =>
            {
                if (block == null || block.GameLogic == null) return false;
                Log.Info("checkbox.Getter");

                var logic = block.GameLogic.GetAs<ProtectionBlock>();
                bool isSet = logic != null ? logic.IsOptionSet : false;
                return isSet;
            };

            checkbox.Setter = (block, value) =>
            {
                Log.Info("checkbox.Setter: setting value to: " + value);

                var logic = block.GameLogic.GetAs<ProtectionBlock>();

                if (logic == null) return;

                logic.IsOptionSet = value;
            };

            controls.Add(checkbox);

            Log.Info("Added checkbox somewhere", "Added checkbox somewhere", 500);

            // Add a slider, hopefully.
            IMyTerminalControlSlider slider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTextPanel>("Sliderry");
            slider.Title = MyStringId.GetOrCompute("sleet");
            slider.Tooltip = MyStringId.GetOrCompute("slip from side to side");
            // SpeedMultiplier.SupportsMultipleBlocks = true;
            // SpeedMultiplier.Enabled = HasBlockLogic;
            // SpeedMultiplier.Visible = HasBlockLogic;
            slider.SetLimits(0.5f, 2.0f);
            slider.Getter = (block) =>
            {
                Log.Info("slider.Getter");
                return 0.5f;
            };

            slider.Setter = (block, value) =>
            {
                Log.Info("slider.Setter: " + value);
            };

            controls.Add(slider);
            // SpeedMultiplier.Getter = (Block) => BlockReturn(Block, x => x.SpeedMultiplier);
   // SpeedMultiplier.Setter = (Block, NewSpeed) => BlockAction(Block, x => x.SpeedMultiplier = (int)NewSpeed);
   // SpeedMultiplier.Writer = (Block, Info) => Info.Append($"x{BlockReturn(Block, x => x.SpeedMultiplier)}");
   // return SpeedMultiplier;

        }
        catch (Exception e) {
            Log.Error(e);
        }
    }

    void CreateTerminalControls() {
        if(createdTerminalControls)
            return;

        Log.Info("CreateTerminalControls", "CreateTerminalControls", 500);

        createdTerminalControls = true;

        MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControls_CustomControlGetter;
    }
}

}
