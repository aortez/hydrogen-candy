/*
This script will tell you when any functional blocks in your grid (ship or base)
take enough damage to render them non-functional.

It will display this info on a block named "statusPanel".

This block can either be an LCD or a display on the cockpit.

If it is a cockpit with multiple displays, you can specify the index you'd like
to see the status on via the Custom Data for the "statusPanel" block by adding a
line like so (in the following example, the screen index is 2):
statusPanel 2

Screen indexes are zero-based, meaning the first screen is 0, second is 1, etc.
*/

const string statusPanelName = "statusPanel";

// Blocks attached to the grid construct.
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

// The surface to write output to.
IMyTextSurface surface = null;

public void updateCache() {
    // Find all blocks attached to the construct.
    GridTerminalSystem.GetBlocksOfType(blocks, block => block.IsSameConstructAs(Me));

    // Find a display surface.
    IMyTextPanel panel = GridTerminalSystem.GetBlockWithName(statusPanelName) as IMyTextPanel;
    if (panel != null) {
        Echo("found panel");
        surface = (IMyTextSurface)panel;
    } else {
        Echo("panel not found");

        IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName(statusPanelName) as IMyCockpit;
        if (cockpit != null) {
            Echo("cockpit found");

            // Figure out if the user has specified which display index to use.
            int surfaceIndex = 0;
            int separatorPos = cockpit.CustomData.LastIndexOf(statusPanelName);
            if (separatorPos >= 0) {
                Echo("custom data: " + cockpit.CustomData);
                try
                {
                    string indexString = cockpit.CustomData[statusPanelName.Length + 1].ToString();
                    Echo("index char: " + indexString);
                    surfaceIndex = Int32.Parse(indexString);
                }
                catch (Exception e)
                {
                    Echo("Error parsing surface index from custom data: " + cockpit.CustomData + ": " + e);
                }
                Echo("surface index: " + surfaceIndex);

            } else {
                Echo("custom data, separator not found...: " + cockpit.CustomData + ", separatorPos: " + separatorPos);
            }

            surface = cockpit.GetSurface(surfaceIndex);
        } else {
            Echo("cockpit not found");
        }

        if (surface == null) {
            Echo("no surface found");
            return;
        }
    }
}

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    updateCache();
}

TimeSpan cacheRefreshPeriod = new TimeSpan(0, 0, 5);
TimeSpan timeSinceLastCacheUpdate = new TimeSpan(0, 0, 0);

public void Main(string argument, UpdateType updateType)
{
    timeSinceLastCacheUpdate += Runtime.TimeSinceLastRun;
    Echo ("LastRunTimeMs: " + Runtime.LastRunTimeMs + ", timeSinceLastCacheUpdate: " + timeSinceLastCacheUpdate);
    if (timeSinceLastCacheUpdate > cacheRefreshPeriod) {
        updateCache();
        timeSinceLastCacheUpdate = new TimeSpan(0, 0, 0);
    }

    if (surface == null) {
        Echo("no output surface found");
        return;
    } else if (blocks == null) {
        Echo("no blocks to monitor status of found");
        return;
    }

    // If they are damaged, tell us!
    StringBuilder outputMessage = new StringBuilder();
    foreach (IMyTerminalBlock block in blocks) {
        if (!(block.IsWorking || block.IsFunctional)) {
            // TODO: add a line break if the message is too long?
            outputMessage.AppendLine(block.CustomName);
        }
    }

    // If all is good, say so.
    if (outputMessage.Length == 0) {
        outputMessage.AppendLine(":)");
        surface.FontSize = 10;
        surface.BackgroundColor = Color.Black;
    } else {
        surface.FontSize = 2;
        surface.BackgroundColor = new Color(125, 0, 0, 50);
    }

    surface.ContentType = ContentType.TEXT_AND_IMAGE;
    surface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
    surface.WriteText(outputMessage);
}
