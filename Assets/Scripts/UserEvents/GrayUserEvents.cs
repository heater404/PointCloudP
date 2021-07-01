using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrayUserEvents : UserEventBase
{
    protected override string ToolTipFormat(Vector2Int sn, float value)
    {
        return $"{sn}:\n{value}LSB";
    }
}
