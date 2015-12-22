using UnityEngine;
using System.Collections;

public class ScriptQuestEvaluate : Script
{
    public override bool Execute()
    {
        return Manager.instance.EvaluateQuests();
    }
}
