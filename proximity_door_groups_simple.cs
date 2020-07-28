/*
Usage:
Create groups of sensors and doors that you wish to automatically open and close.
All doors in the group will automatically open when any sensor detects an entity
within range. The groups must have the word "autodoor" in them. The groups can
have an arbitrary number of sensors or doors, with the simplest group being
a single door and a single sensor.

Copy/paste this script into a Program Block and run it.
Then go name the doors and sensors and add them to your "autodoor" groups.

Simplest Example:
* Door: "door"
* Sensor: "sensor"
* Group contains both of the above and is named "autodoor"

Example wth a bigger group:
* Doors: "door", "door2", "door3"
* Sensor: "sensor", "sensor2"
* Group contains all of the above and is named "lots of autodoors"
*/

const string autoGroupTag = "autodoor";

List<MyDetectedEntityInfo> entitiesDetected = new List<MyDetectedEntityInfo>();
List<IMyBlockGroup> autoGroups = new List<IMyBlockGroup>();
List<IMyDoor> doors = new List<IMyDoor>();
List<IMySensorBlock> sensors = new List<IMySensorBlock>();

int cacheRefreshCounter = 0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    updateCache();
}

public void updateCache() {
//    Echo("cacheRefreshCounter: " + cacheRefreshCounter);
    if (cacheRefreshCounter % 10 == 0) { // this interval is sort of arbitrary ATM
        GridTerminalSystem.GetBlockGroups(autoGroups, group => group.Name.Contains(autoGroupTag));
    }
    cacheRefreshCounter++;
}

public void Main(string argument, UpdateType updateType)
{
    updateCache();

    foreach (IMyBlockGroup group in autoGroups) {
        group.GetBlocksOfType(sensors);
        if (sensors.Count < 1) continue;

        group.GetBlocksOfType(doors);
        if (doors.Count < 1) continue;

        bool isAnythingDetected = false;
        foreach (IMySensorBlock sensor in sensors) {
            if (sensor.IsActive) {
                isAnythingDetected = true;
                break;
            }
        }

        foreach (IMyDoor door in doors) {
            if (isAnythingDetected) {
                door.OpenDoor();
            } else {
                door.CloseDoor();
            }
        }
    }
    Echo ("LastRunTimeMs: " + Runtime.LastRunTimeMs);
}
