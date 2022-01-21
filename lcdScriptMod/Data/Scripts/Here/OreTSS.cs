using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

// Avoid including ingame namespaces because they cause ambiguity errors, instead, do aliases:
using IMyGridTerminalSystem = Sandbox.ModAPI.Ingame.IMyGridTerminalSystem;
using IMyTerminalBlock = Sandbox.ModAPI.Ingame.IMyTerminalBlock;

using IMyInventory = VRage.Game.ModAPI.Ingame.IMyInventory;
using MyItemType = VRage.Game.ModAPI.Ingame.MyItemType;

using Digi;

namespace AllanTTS
{

// Text Surface Scripts (TSS) can be selected in any LCD's scripts list.
// These are meant as fast no-sync (sprites are not sent over network) display scripts, and the Run() method only executes player-side (no DS).
// You can still use a session comp and access it through this to use for caches/shared data/etc.
//
// The display name has localization support aswell, same as a block's DisplayName in SBC.
public class IngotsAndOresTSS : MyTSSCommon
{
    protected ItemGroups mode;

    // Update frequency, in number of frames.
    public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update100;

    // The TerminalBlock that this TSS lives in.
    private readonly Sandbox.ModAPI.IMyTerminalBlock TerminalBlock;

    public IngotsAndOresTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
        mode = ItemGroups.IngotsAndOres;
        // Called when script is created.
        // This class is instanced per LCD that uses it, which means the same block can have multiple instances of this script aswell (e.g. a cockpit with all its screens set to use this script).
        TerminalBlock = (Sandbox.ModAPI.IMyTerminalBlock)block; // internal stored m_block is the ingame interface which has no events, so can't unhook later on, therefore this field is required.
        TerminalBlock.OnMarkForClose += BlockMarkedForClose; // required if you're gonna make use of Dispose() as it won't get called when block is removed or grid is cut/unloaded.
    }

    public override void Dispose()
    {
        base.Dispose(); // do not remove
        TerminalBlock.OnMarkForClose -= BlockMarkedForClose;

        // Called when script is removed for any reason, so that you can clean up stuff if you need to.
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
            if (!TssSession.Instance.Cache.IsFresh) {
                TssSession.Instance.Cache.RefreshCache(TerminalBlock.CubeGrid);
            } else {
                Log.Info("using cache", "using cache", 500);
                MyAPIGateway.Utilities.ShowNotification("USING CACHE!!!!");
            }

            // hold L key to see how the error is shown, remove this after you've played around with it =)
            if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.L)) {
                throw new Exception("Oh noes an error :}");
            }

            if (mode == ItemGroups.Ingots) {
                Draw(TssSession.Instance.Cache.IngotsMessage.ToString());
            }
            else if (mode == ItemGroups.Ores) {
                Draw(TssSession.Instance.Cache.OresMessage.ToString());
            }
            else if (mode == ItemGroups.IngotsAndOres) {
                Draw(TssSession.Instance.Cache.OresMessage.ToString() + "\n" + TssSession.Instance.Cache.IngotsMessage.ToString());
            }
            else {
                throw new Exception("Oh noes an error :(...");
            }

        }
        catch(Exception e)
        {
            // Display the error to the user.
            DrawError(e);
        }
    }

    // Drawing sprites works exactly like in PB API.
    // Therefore this guide applies: https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites

    // there are also some helper methods from the MyTSSCommon that this extends.
    // like: AddBackground(frame, Surface.ScriptBackgroundColor); - a grid-textured background

    // the colors in the terminal are Surface.ScriptBackgroundColor and Surface.ScriptForegroundColor, the other ones without Script in name are for text/image mode.

    void Draw(String message)
    {
        Vector2 screenSize = Surface.SurfaceSize;
        Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

        float fontScale = 1.0f;
        float length = screenSize.Length();
        if (length < 500.0f) {
            fontScale = 0.5f;
        }

        var frame = Surface.DrawFrame();
        var text = MySprite.CreateText(message, "Monospace", Surface.ScriptForegroundColor, fontScale, TextAlignment.LEFT);
        text.Position = screenCorner + new Vector2(16, 16); // 16px from topleft corner of the visible surface
        frame.Add(text);

        // add more sprites and stuff

        frame.Dispose(); // send sprites to the screen
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

        // add more sprites and stuff

        frame.Dispose(); // send sprites to the screen
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
