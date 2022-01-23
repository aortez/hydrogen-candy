using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

using IMyGridTerminalSystem = Sandbox.ModAPI.Ingame.IMyGridTerminalSystem;
using IMyInventory = VRage.Game.ModAPI.Ingame.IMyInventory;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

using Digi;

namespace AllanTTS
{

public class IngotsAndOresTSS : MyTSSCommon
{
    protected ItemGroups mode;

    // Update frequency, in number of frames.
    public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update100;

    // The TerminalBlock that this TSS lives in.
    private readonly Sandbox.ModAPI.IMyTerminalBlock TerminalBlock;

    private static bool createdTerminalControls = false;

    public IngotsAndOresTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        mode = ItemGroups.IngotsAndOres;
        // Called when script is created.
        // This class is instanced per LCD that uses it, which means the same block can have multiple instances of this script aswell (e.g. a cockpit with all its screens set to use this script).
        TerminalBlock = (Sandbox.ModAPI.IMyTerminalBlock)block; // internal stored m_block is the ingame interface which has no events, so can't unhook later on, therefore this field is required.
        TerminalBlock.OnMarkForClose += BlockMarkedForClose; // required if you're gonna make use of Dispose() as it won't get called when block is removed or grid is cut/unloaded.

        CreateTerminalControls();
    }

    // Called when script is removed for any reason, do clean up here.
    public override void Dispose()
    {
        base.Dispose(); // do not remove
        TerminalBlock.OnMarkForClose -= BlockMarkedForClose;
    }

    void BlockMarkedForClose(IMyEntity ent)
    {
        Dispose();
    }

    // Gets called at the rate specified by NeedsUpdate.
    // It can't run every tick because the LCD is capped at 6fps anyway.
    public override void Run()
    {
        try
        {
            base.Run();

            // If cache is stale, refresh it.
            if (!TssSession.Instance.Cache.ContainsKey(TerminalBlock.CubeGrid.EntityId)) {
                throw new Exception("Cache should not be null!");
            }
            GridItems cache = TssSession.Instance.Cache[TerminalBlock.CubeGrid.EntityId];
            if (!cache.IsFresh) {
                cache.RefreshCache(TerminalBlock.CubeGrid);
            }

            // hold L key to see how the error is shown.
            if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.L)) {
                throw new Exception("Oh noes you pressed L");
            }

            if (mode == ItemGroups.CargoSpace) {
                Draw(cache.CargoMessage.ToString());
            }
            else if (mode == ItemGroups.Ingots) {
                Draw(cache.IngotsMessage.ToString());
            }
            else if (mode == ItemGroups.IngotsAndOres) {
                Draw(
                    cache.OresMessage.ToString()
                    + "\n" + cache.IngotsMessage.ToString()
                    + "\n" + cache.CargoMessage.ToString()
                );
            }
            else if (mode == ItemGroups.Ores) {
                Draw(cache.OresMessage.ToString());
            }
            else {
                throw new Exception("Unhandled TSS \"mode\": " + mode);
            }
        }
        catch(Exception e)
        {
            // Display the error to the user.
            DrawError(e);
        }
    }

    void Draw(String message)
    {
        Stuff settings = TssSession.Instance.TssSettings[TerminalBlock.EntityId];

        Vector2 screenSize = Surface.SurfaceSize;
        Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

        var frame = Surface.DrawFrame();
        var text = MySprite.CreateText(message, "White", Surface.ScriptForegroundColor, settings.Scale, TextAlignment.LEFT);
        text.Position = screenCorner + new Vector2(16, 16); // 16px from topleft corner of the visible surface
        // Log.Info("text.Size: " + (float)text.Size?.Length(), "text.Size: " + (float)text.Size?.Length(), 500);
        frame.Add(text);

        frame.Dispose();
    }

    // The original example Draw() method.
    void Draw()
    {
        Vector2 screenSize = Surface.SurfaceSize;
        Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

        var frame = Surface.DrawFrame();
        var text = MySprite.CreateText("Hi!", "Monospace", Surface.ScriptForegroundColor, 1f, TextAlignment.LEFT);
        text.Position = screenCorner + new Vector2(16, 16); // 16px from topleft corner of the visible surface
        frame.Add(text);

        frame.Dispose();
    }

    void DrawError(Exception e)
    {
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

        try // first try printing the error on the LCD
        {
            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

            var frame = Surface.DrawFrame();

            var bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", null, null, Color.Black);
            frame.Add(bg);

            var text = MySprite.CreateText($"ERROR: {e.Message}\n{e.StackTrace}\n\nPlease send screenshot of this to mod author.\n{MyAPIGateway.Utilities.GamePaths.ModScopeName}", "White", Color.Red, 0.7f, TextAlignment.LEFT);
            text.Position = screenCorner + new Vector2(16, 16);
            frame.Add(text);

            frame.Dispose();
        }
        catch(Exception e2)
        {
            MyLog.Default.WriteLineAndConsole($"Also failed to draw error on screen: {e2.Message}\n{e2.StackTrace}");
            if(MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }
    }

    void CreateTerminalControls() {
        // Maybe create the data cache for this grid.
        if (!TssSession.Instance.Cache.ContainsKey(TerminalBlock.CubeGrid.EntityId)) {
            TssSession.Instance.Cache.Add(TerminalBlock.CubeGrid.EntityId, new GridItems());
        }

        if (TssSession.Instance.TssSettings.ContainsKey(TerminalBlock.EntityId)) return;

        Log.Info("\nCandidate DefinitionDisplayNameText: " + TerminalBlock.DefinitionDisplayNameText);
        Log.Info("Candidate DetailedInfo: " + TerminalBlock.DetailedInfo);
        Log.Info("Candidate DisplayName: " + TerminalBlock.DisplayName);
        Log.Info("Candidate EntityId: " + TerminalBlock.EntityId);
        Log.Info("Candidate Name: " + TerminalBlock.Name);
        Log.Info("Registering EntityID: " + TerminalBlock.EntityId, "Registering entityID: " + TerminalBlock.EntityId, 500);
        Stuff stuff = new Stuff(false, 0.8f);
        if (TerminalBlock.DefinitionDisplayNameText == "LCD Panel Large (Allan)") {
            stuff.Scale = 1.2f;
        }
        TssSession.Instance.TssSettings.Add(TerminalBlock.EntityId, stuff);

        if(createdTerminalControls)
            return;

        createdTerminalControls = true;
        MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControls_CustomControlGetter;
    }

    private static void TerminalControls_CustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
    {
        bool found = TssSession.Instance.TssSettings.ContainsKey(block.EntityId);
        Log.Info("found: " + found, "found: " + found, 500);
        if (!found) {
            return;
        }

        Stuff settings = TssSession.Instance.TssSettings[block.EntityId];

        try {
            // Add a checkbox.  It doesn't do anything yet.
            var checkbox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyTerminalBlock>("DO somethign");
            checkbox.Title = MyStringId.GetOrCompute("Yeet");
            checkbox.Tooltip = MyStringId.GetOrCompute("You guessed it.\nNope, actually try again.");
            checkbox.Getter = (b) =>
            {
                // need some global state to hold the scale factor that this button controls
                // if (b == null || b.GameLogic == null) return false;
                Log.Info("checkbox.Getter");
                return settings.Big;
            };

            checkbox.Setter = (b, value) =>
            {
                Log.Info("checkbox.Setter: setting value to: " + value);

                settings.Big = value;
            };

            controls.Add(checkbox);

            Log.Info("Added checkbox somewhere", "Added checkbox somewhere", 500);

            // // Add a slider.
            IMyTerminalControlSlider slider = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>("Sliderry");
            slider.Title = MyStringId.GetOrCompute("LCD Script Scale");
            slider.Tooltip = MyStringId.GetOrCompute("slip from side to side");
            slider.SetLimits(0.25f, 2.5f);

            slider.Getter = (b) =>
            {
                return settings.Scale;
            };

            slider.Setter = (b, value) =>
            {
                settings.Scale = value;
            };

            controls.Add(slider);
        }
        catch (Exception e) {
            Log.Error(e);
        }
    }

}

[MyTextSurfaceScript("CargoSpace", "CargoSpace")]
public class MyCargoSpace : IngotsAndOresTSS {
    public MyCargoSpace(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        base.mode = ItemGroups.CargoSpace;
        Log.Info("new CargoSpace TSS created");
    }
}

[MyTextSurfaceScript("Ores", "Ores")]
public class Ores : IngotsAndOresTSS {
    public Ores(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        base.mode = ItemGroups.Ores;
        Log.Info("new Ores TSS created");
    }
}

[MyTextSurfaceScript("IngotsAndOres", "IngotsAndOres")]
public class IngotsAndOres : IngotsAndOresTSS {
    public IngotsAndOres(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        base.mode = ItemGroups.IngotsAndOres;
        Log.Info("new Ingots and Ores TSS created");
    }
}

[MyTextSurfaceScript("Ingots", "Ingots")]
public class Ingots : IngotsAndOresTSS {
    public Ingots(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        base.mode = ItemGroups.Ingots;
        Log.Info("new Ingots TSS created");
    }
}

}
