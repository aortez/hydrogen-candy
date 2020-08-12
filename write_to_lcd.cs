public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateType)
{
    IMyTextSurfaceProvider lcdButtonPanel = GridTerminalSystem.GetBlockWithName("The Button") as IMyTextSurfaceProvider;
    if (lcdButtonPanel == null) {
        Echo ("couldn't find the button panel");
        return;
    }

    Echo ("found the button panel: " + lcdButtonPanel.ToString());

    IMyTextSurface drawingSurface = lcdButtonPanel.GetSurface(0);

    Echo ("I have something to draw on: " + (drawingSurface != null));

    drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
    drawingSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
    drawingSurface.WriteText("hey it works!");
}
