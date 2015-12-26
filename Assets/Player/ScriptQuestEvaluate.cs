using UnityEngine;
using System.Collections;

public class ScriptQuestEvaluate : Script
{
    static bool s_debugOverrideNext = false;

    public virtual void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            s_debugOverrideNext = true;
        }
    }

    public override bool Execute()
    {
        if (s_debugOverrideNext)
        {
            s_debugOverrideNext = false;
            return true;
        }

        return Manager.instance.EvaluateQuests();
    }
}
