using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI.Ingame;

using IMyCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;
using IMyCubeGrid = VRage.Game.ModAPI.IMyCubeGrid;
using IMyEntity = VRage.Game.ModAPI.Ingame.IMyEntity;
using IMyInventory = VRage.Game.ModAPI.Ingame.IMyInventory;
using IMySlimBlock = VRage.Game.ModAPI.IMySlimBlock;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

using Digi;

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

class PowerStat {

    public void AddBlock(IMyEntity block)
    {
        Log.Info("1");
        if (block is IMyPowerProducer) {
            Log.Info("2");
            OutputCurrentMw += ((IMyPowerProducer)block).CurrentOutput;
            OutputMaxMw += ((IMyPowerProducer)block).MaxOutput;
        }
        Log.Info("3");
        if (block is IMyBatteryBlock) {
            Log.Info("4");
            StoredPowerCurrent += ((IMyBatteryBlock)block).CurrentStoredPower;
            StoredPowerMax += ((IMyBatteryBlock)block).MaxStoredPower;
            InputCurrent += ((IMyBatteryBlock)block).CurrentInput;
        }
    }

    // In MegaWatts.
    public double InputCurrent = 0;

    public double OutputCurrentMw = 0;
    public double OutputMaxMw = 0;

    public double StoredPowerCurrent = 0;
    public double StoredPowerMax = 0;
}

// Mode specified for this script.
// This controls what is processed/displayed.
public enum ItemGroups
{
    CargoSpace,
    Ingots,
    IngotsAndOres,
    Ores,
    Power,
}

public class GridItems {
    public StringBuilder CargoMessage = new StringBuilder();
    public StringBuilder IngotsMessage = new StringBuilder();
    public StringBuilder OresMessage = new StringBuilder();
    public StringBuilder PowerMessage = new StringBuilder();

    public bool IsFresh = false;

    private CargoSpace cargoSpace = new CargoSpace();

    private PowerStat power = new PowerStat();

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

    public void RefreshCache(IMyCubeGrid grid) {
        // TODO clear instead of realloc?  It is a struct, does that make a difference?
        cargoSpace = new CargoSpace();

        power = new PowerStat();

        // Fetch all blocks in grid.
        // TODO Is there some way to avoid this alloc on each update?
        List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        grid.GetBlocks(blocks);

        // Check each block and see if it has something that we're tracking.
        foreach (IMySlimBlock slimBlock in blocks)
        {
            IMyCubeBlock block = slimBlock.FatBlock;

            power.AddBlock(block);

            if (block == null || !block.HasInventory) {
                 continue;
            }

            cargoSpace += new CargoSpace(block);

            foreach (Item item in ingots)
            {
                item.Count += CountItemType(block, item.Type);
            }

            foreach (Item item in ores)
            {
                item.Count += CountItemType(block, item.Type);
            }
        }

        // Build the output messages...
        // Cargo Space...
        CargoMessage = new StringBuilder();
        CargoMessage.AppendLine("--- Cargo Space---");
        int numCharsInGauge = 10;
        CargoMessage.Append(printGauge(numCharsInGauge, cargoSpace.FillPercent / 100.0));
        CargoMessage.AppendLine(" " + cargoSpace.FillPercent.ToString("#,#0.0") + "% Full");
        CargoMessage.AppendLine("Current: " + cargoSpace.CurrentLiters.ToString("#,#0.0") + " liters");
        CargoMessage.AppendLine("Max: " + cargoSpace.MaxLiters.ToString("#,#0.0") + " liters");

        // Ingots...
        IngotsMessage = new StringBuilder();
        IngotsMessage.AppendLine("--- Ingots ---");
        foreach (Item item in ingots)
        {
            MaybePrintCount(IngotsMessage, item.Count, item.DisplayName);
            item.Count = 0;
        }

        // Ores...
        OresMessage = new StringBuilder();
        OresMessage.AppendLine("--- Ores ---");
        foreach (Item item in ores)
        {
            MaybePrintCount(OresMessage, item.Count, item.DisplayName);
            item.Count = 0;
        }

        // Power...
        PowerMessage = new StringBuilder();
        PowerMessage.AppendLine("--- Power ---");
        PowerMessage.AppendLine((power.StoredPowerCurrent / power.StoredPowerMax * 100).ToString("#,#0.0") + " %: " + printGauge(numCharsInGauge, power.StoredPowerCurrent / power.StoredPowerMax));
        PowerMessage.AppendLine("Stored: " + power.StoredPowerCurrent.ToString("#,#0.0") + "/" + power.StoredPowerMax.ToString("#,#0.0") + " Mw");
        PowerMessage.AppendLine("Input: " + power.InputCurrent.ToString("#,#0.0") + " Mw");
        PowerMessage.AppendLine("Output: " + power.OutputCurrentMw.ToString("#,#0.0") + "/" + power.OutputMaxMw.ToString("#,#0.0") + " Mw");

        IsFresh = true;
    }

    public String printGauge(int numCharsLong, double fillRatio) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        for (int i = 0; i < numCharsLong; i++) {
            if ((int)(fillRatio * numCharsLong) > i) {
                sb.Append("+");
            } else {
                sb.Append(" ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    // Basic representation of filled vs total cargo capacity.
    public struct CargoSpace
    {
        public CargoSpace(double currentLiters, double maxLiters)
        {
            CurrentLiters = currentLiters;
            MaxLiters = maxLiters;
        }

        public CargoSpace(IMyEntity block) {
            CurrentLiters = 0;
            MaxLiters = 0;
            for (int i = 0; i < block.InventoryCount; i++)
            {
                IMyInventory inventory = block.GetInventory(i);
                CurrentLiters += (double)inventory.CurrentVolume;
                MaxLiters += (double)inventory.MaxVolume;
            }
        }

        // Overload operator+ for easier summing of cargos.
        public static CargoSpace operator +(CargoSpace a, CargoSpace b)
             => new CargoSpace(a.CurrentLiters + b.CurrentLiters, a.MaxLiters + b.MaxLiters);

        public double CurrentLiters;
        public double MaxLiters;
        public double FillPercent {
            get
            {
                if (MaxLiters > 0) {
                    return CurrentLiters / MaxLiters * 100;
                } else {
                    return 100;
                }
            }
        }
    }

    private long CountItemType(IMyEntity block, MyItemType type)
    {
        long count = 0;
        for (int i = 0; i < block.InventoryCount; i++)
        {
            count += (long)block.GetInventory(i).GetItemAmount(type);
        }
        return count;
    }

    private void MaybePrintCount(StringBuilder stringBuilder, long count, String name)
    {
        if (count <= 0) {
            return;
        }

        stringBuilder.AppendLine(name + ": " + RawAmountToWeight(count));
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

}
