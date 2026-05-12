using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public float checkpointX;
    public float checkpointY;
    public List<string> inventoryItemNames = new List<string>();
    public int keyCount;
    public List<string> activatedCheckpoints = new List<string>();
    public bool isBayonetTrapDeactivated;
}