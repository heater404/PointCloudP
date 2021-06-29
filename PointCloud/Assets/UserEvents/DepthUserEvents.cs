using UnityEngine;
public class DepthUserEvents : UserEventBase
{
    protected override string ToolTipFormat(Vector2Int sn, float value)
    {
        return $"{sn}:\n{value}mm";
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
