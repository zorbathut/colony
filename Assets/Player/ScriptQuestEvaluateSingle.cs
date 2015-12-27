using UnityEngine;
using System.Collections;

public class ScriptQuestEvaluateSingle : Script
{
    [SerializeField] Quest m_Quest;
    public override bool Execute()
    {
        if (ScriptQuestEvaluate.ConsumeDebugOverride())
        {
            return true;
        }

        return m_Quest.IsComplete();
    }
}
