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

/*
Cache is held by the LC
Cache probes are done against all other blocks.

*/

namespace AllanTTS
{

public class StuffCache {
    public StringBuilder IngotsMessage = new StringBuilder();
    public StringBuilder OresMessage = new StringBuilder();

    // Ore Item types.
    private List<Item> ores = new List<Item>() {
        new Item("Cobalt", "Cobalt", "MyObjectBuilder_Ore"),
        new Item("Gold", "Gold", "MyObjectBuilder_Ore"),
        new Item("Ice", "Ice", "MyObjectBuilder_Ore"),
        new Item("Iron", "Iron", "MyObjectBuilder_Ore"),
        new Item("Magnesium", "Magnesium", "MyObjectBuilder_Ore"),
        new Item("Nickel", "Nickel", "MyObjectBuilder_Ore"),
        new Item("Platinum", "Platinum", "MyObjectBuilder_Ore"),
        new Item("Scrap", "Scrap", "MyObjectBuilder_Ore"),
        new Item("Silicon", "Silicon", "MyObjectBuilder_Ore"),
        new Item("Silver", "Silver", "MyObjectBuilder_Ore"),
        new Item("Stone", "Stone", "MyObjectBuilder_Ore"),
        new Item("Uranium", "Uranium", "MyObjectBuilder_Ore")
    };

    // Ore Item types.
    private List<Item> ingots = new List<Item>() {
        new Item("Cobalt", "Cobalt", "MyObjectBuilder_Ingot"),
        new Item("Gold", "Gold", "MyObjectBuilder_Ingot"),
        new Item("Gravel", "Stone", "MyObjectBuilder_Ingot"),
        new Item("Iron", "Iron", "MyObjectBuilder_Ingot"),
        new Item("Magnesium", "Magnesium", "MyObjectBuilder_Ingot"),
        new Item("Nickel", "Nickel", "MyObjectBuilder_Ingot"),
        new Item("Platinum", "Platinum", "MyObjectBuilder_Ingot"),
        new Item("Silicon", "Silicon", "MyObjectBuilder_Ingot"),
        new Item("Silver", "Silver", "MyObjectBuilder_Ingot"),
        new Item("Uranium", "Uranium", "MyObjectBuilder_Ingot")
    };

    // Blocks on this grid.
    private List<IMySlimBlock> blocks = new List<IMySlimBlock>();

    // Is the cache fresh? The holder of this cache needs to maintain this state
    // if they want it to be valid.
    public bool IsFresh;

    public void RefreshCache(IMyCubeGrid grid) {
        Log.Info("refreshing cache");

        // Fetch all blocks in grid.
        // IMyGridTerminalSystem gridTerminal = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((Sandbox.ModAPI.IMyCubeGrid)grid);
        // ((Sandbox.ModAPI.IMyCubeGrid)grid).GetBlocks(blocks);
        grid.GetBlocks(blocks);

        // Check to see if they have what we're looking for.
        foreach (IMySlimBlock slimBlock in blocks)
        {
            IMyCubeBlock block = slimBlock.FatBlock;
            if (block == null || !block.HasInventory) {
                 continue;
            }

            foreach (Item item in ingots)
            {
                item.Count += CountItemType(block, item.Type);
            }

            foreach (Item item in ores)
            {
                item.Count += CountItemType(block, item.Type);
            }
        }

        OresMessage = new StringBuilder();
        OresMessage.AppendLine("--- Ores ---");
        foreach (Item item in ores)
        {
            MaybePrintCount(OresMessage, item);
            item.Count = 0;
        }

        IngotsMessage = new StringBuilder();
        IngotsMessage.AppendLine("--- Ingots ---");
        foreach (Item item in ingots)
        {
            MaybePrintCount(IngotsMessage, item);
            item.Count = 0;
        }
    }

    private void MaybePrintCount(StringBuilder stringBuilder, Item item)
    {
        if (item.Count <= 0) {
            return;
        }

        stringBuilder.AppendLine(item.DisplayName + ": " + RawAmountToWeight(item.Count));
    }

    private long CountItemType(IMyEntity block, MyItemType type)
    {
        long count = 0;

        if (!block.HasInventory)
        {
            return count;
        }

        for (int i = 0; i < block.InventoryCount; i++)
        {
            IMyInventory inventory = block.GetInventory(i);
            count += (long)inventory.GetItemAmount(type);
        }
        return count;
    }

    private string RawAmountToWeight(long rawAmount)
    {
        if (rawAmount < 1000) {
            return (rawAmount / 1.0f).ToString("#,#0.0") + " kg";
        } else if (rawAmount < 1000000) {
            return (rawAmount / 1000.0f).ToString("#,#0.0") + " k";
        } else {
            return (rawAmount / 1000000.0f).ToString("#,#0.0") + " Gg";
        }
    }
}

// change MyObjectBuilder_BatteryBlock to the block type you're using, it must be the exact type, no inheritence.
[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "AllanLargeLCDPanel")]
public class ProtectionBlock : MyGameLogicComponent
{
    public StuffCache Cache = new StuffCache();

    public bool IsOptionSet;

    private IMyFunctionalBlock block;

    private bool createdTerminalControls;

    // this method is called async! always do stuff in the first update unless you're sure it must be in Init().
    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    {
        NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        Cache.IsFresh = false;
        Log.Info("ProtectionBlock.Init", "ProtectionBlock.Init", 500);
    }

    public override void UpdateOnceBeforeFrame() // first update of the block
    {
        block = (IMyFunctionalBlock)Entity;

        if (block.CubeGrid?.Physics == null) // ignore projected and other non-physical grids
            return;

        ProtectionSession.Instance?.ProtectionBlocks.Add(block);

        CreateTerminalControls();

        Log.Info("UpdateOnceBeforeFrame", "UpdateOnceBeforeFrame", 500);
    }

    public override void Close() // called when block is removed for whatever reason (including ship despawn)
    {
        ProtectionSession.Instance?.ProtectionBlocks.Remove((IMyFunctionalBlock)Entity);
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
