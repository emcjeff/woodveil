using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class Blueprint
{
    public string itemName;        // Internal ID used by Inventory (e.g., "ArrowUI")
    public string displayName;     // Clean name shown to the player (e.g., "Arrow")
    public string Req1;
    public string Req2;

    public int Req1amount;
    public int Req2amount;

    public int numOfRequirements;

    // Updated Constructor to include 'displayName' as the second parameter
    public Blueprint(string name, string display, int reqNUM, string R1, int R1num, string R2, int R2num)
    {
        itemName = name;
        displayName = display; // Assign the clean name
        numOfRequirements = reqNUM;

        Req1 = R1;
        Req2 = R2;

        Req1amount = R1num;
        Req2amount = R2num;
    }
}