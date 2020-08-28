/*
Usage:
This script will automatically open a door when some is within sensor range
and it will also _close_ the door when there is no one in sensor range.

Name your door "autodoor" and your sensor "autosensor", then recompile the script.
*/

const string doorName = "autodoor";
const string sensorName = "autosensor";

IMyDoor door;
IMySensorBlock sensor;
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

    if (sensor.IsActive) {
        door.OpenDoor();
    } else {
        door.CloseDoor();
    }
}
