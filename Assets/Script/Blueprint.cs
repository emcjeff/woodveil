using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// --- DO NOT REMOVE THIS CLASS ---
// This is what defines the 7 arguments (name, reqs, amounts) so Unity can read your blueprints!
[System.Serializable]
public class Blueprint
{
    public string nameUI;
    public string itemName;
    public int numOfRequirements;

    public string Req1;
    public int Req1amount;

    public string Req2;
    public int Req2amount;

    public Blueprint(string name, string item, int reqNum, string r1, int r1Amt, string r2, int r2Amt)
    {
        nameUI = name;
        itemName = item;
        numOfRequirements = reqNum;
        Req1 = r1;
        Req1amount = r1Amt;
        Req2 = r2;
        Req2amount = r2Amt;
    }
}