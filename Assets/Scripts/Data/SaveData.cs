using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public float posX;
    public float posY;
    public List<string> inventoryItemNames = new List<string>();
    public int keyCount;
}