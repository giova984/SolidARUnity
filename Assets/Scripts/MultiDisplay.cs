using UnityEngine;
using System.Collections;

public enum DisplayID
{
    Display1 = 0, Display2 = 1, Display3 = 2, Display4 = 3, Display5 = 4, Display6 = 5, Display7 = 6, Display8 = 7
}

public class MultiDisplay
{
    public static DisplayID[] ProjectorToDisplay = { DisplayID.Display1, DisplayID.Display2, DisplayID.Display3, DisplayID.Display4, DisplayID.Display5, DisplayID.Display6, DisplayID.Display7, DisplayID.Display8 };
}