using UnityEngine;
using System.Collections;

public class ScriptQuestEvaluate : Script
{
    static bool s_debugOverrideNext = false;

    public static bool ConsumeDebugOverride()
    {
        bool result = s_debugOverrideNext;
        s_debugOverrideNext = false;
        return result;
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            s_debugOverrideNext = true;
        }
    }

    public override bool Execute()
    {
        if (ConsumeDebugOverride())
        {
            return true;
        }

        return Manager.instance.EvaluateQuests();
    }
}
