public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

List<String> imageList = new List<String>();

TimeSpan flipPeriod = new TimeSpan(0, 0, 5);
TimeSpan timeSinceLastFlip = new TimeSpan(0, 0, 0);

bool shouldShowText = false;

public void Main(string argument, UpdateType updateType)
{
    timeSinceLastFlip += Runtime.TimeSinceLastRun;
    Echo ("s: " + Runtime.LastRunTimeMs + ", timeSinceLastFlip: " + timeSinceLastFlip);
    if (timeSinceLastFlip > flipPeriod) {
        shouldShowText = !shouldShowText;
        timeSinceLastFlip = new TimeSpan(0, 0, 0);
    }

    IMyTextSurfaceProvider lcdButtonPanel = GridTerminalSystem.GetBlockWithName("The Button") as IMyTextSurfaceProvider;
    if (lcdButtonPanel == null) {
        Echo ("couldn't find the button panel");
        return;
    }

    IMyTextSurface drawingSurface = lcdButtonPanel.GetSurface(0);
    if (drawingSurface == null) {
        Echo ("no drawing surface on the block?");
        return;
    }

    // Time to show the text.
    if (shouldShowText) {
        // If it is showing an image currently, copy the image ID for later
        // so we can switch back to showing it.
        if (!String.IsNullOrEmpty(drawingSurface.CurrentlyShownImage)) {
            drawingSurface.GetSelectedImages(imageList);

            // After we have the list of images, clear them so we can see our output
            // text.
            drawingSurface.ClearImagesFromSelection();
        }
        drawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
        drawingSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
        drawingSurface.WriteText("showing text");
    } else {
        // Show whatever was there previously...
        drawingSurface.WriteText("");
        drawingSurface.AddImagesToSelection(imageList, true);
    }
}
