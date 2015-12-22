using UnityEngine;
using System.Collections;

public class ScriptQuestAdd : Script
{
    [SerializeField] Quest m_Quest;

    public override bool Execute()
    {
        Manager.instance.AddQuest(m_Quest);

        return true;
    }
}
