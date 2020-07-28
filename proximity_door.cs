/*
Usage:
This script will automatically open a door when some is within sensor range
and it will also _close_ the door when there is no one in sensor range.

Name your door "autodoor" and your sensor "autosensor", then run the script.
*/

const string doorName = "autodoor";
const string sensorName = "autosensor";

IMyDoor door;
IMySensorBlock sensor;
List<MyDetectedEntityInfo> entities = new List<MyDetectedEntityInfo>();
int counter = 0;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    door = GridTerminalSystem.GetBlockWithName(doorName) as IMyDoor;
    sensor = GridTerminalSystem.GetBlockWithName(sensorName) as IMySensorBlock;
}

public void Main(string argument, UpdateType updateType)
{
    if (door == null || sensor == null) return;

    sensor.DetectedEntities(entities);

    if (entities.Count == 0)
    {
        door.ApplyAction("Open_Off");
    } else {
        door.ApplyAction("Open_On");
    }
}
