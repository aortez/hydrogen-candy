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

public class AutoDoorGroup
{
    public AutoDoorGroup(List<IMyDoor> doors, List<IMySensorBlock> sensors, string name)
    {
        Doors = doors;
        Sensors = sensors;
        Name = name;
    }

    public void autoOpenDoors()
    {
        foreach (IMyDoor door in Doors) {
            if (isAnythingDetected()) {
                door.OpenDoor();
            } else {
                door.CloseDoor();
            }
        }
    }

    public bool isAnythingDetected()
    {
        foreach (IMySensorBlock sensor in Sensors) {
            if (sensor.IsActive) {
                return true;
            }
        }
        return false;
    }

    public List<IMySensorBlock> Sensors { get; set;}
    public List<IMyDoor> Doors { get; set; }
    public string Name { get; set; }
}

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    updateCache();
}

List<AutoDoorGroup> autoDoorGroups = new List<AutoDoorGroup>();
List<IMyBlockGroup> autoGroups = new List<IMyBlockGroup>();
public void updateCache() {
    GridTerminalSystem.GetBlockGroups(autoGroups, group => group.Name.Contains(autoGroupTag));

    // Update the cache...
    int autoDoorGroupIndex = 0;
    foreach (IMyBlockGroup group in autoGroups) {
        List<IMySensorBlock> sensors;
        List<IMyDoor> doors;
        // If we need to, allocate new lists.
        if (autoDoorGroupIndex >= autoDoorGroups.Count) {
            doors = new List<IMyDoor>();
            sensors = new List<IMySensorBlock>();
            autoDoorGroups.Add(new AutoDoorGroup(doors, sensors, group.Name));
        } else {
            // Otherwise, just reuse the existing ones.
            doors = autoDoorGroups[autoDoorGroupIndex].Doors;
            sensors = autoDoorGroups[autoDoorGroupIndex].Sensors;
            autoDoorGroups[autoDoorGroupIndex].Name = group.Name;
        }
        group.GetBlocksOfType(sensors);
        group.GetBlocksOfType(doors);
        autoDoorGroupIndex++;
    }

    // Prune any excess groups.
    for (int toRemoveIndex = autoDoorGroupIndex; toRemoveIndex < autoDoorGroups.Count; toRemoveIndex++) {
        autoDoorGroups.RemoveAt(toRemoveIndex);
    }
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

    foreach (AutoDoorGroup autoGroup in autoDoorGroups) {
        autoGroup.autoOpenDoors();
    }
}
