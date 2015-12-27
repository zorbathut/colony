using UnityEngine;
using System.Collections;

public class ScriptReprocessAllStructural : Script
{
    public override bool Execute()
    {
        Manager.instance.ReprocessAllStructural();

        return true;
    }
}
