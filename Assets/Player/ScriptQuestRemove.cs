using UnityEngine;
using System.Collections;

public class ScriptQuestRemove : Script
{
    [SerializeField] Quest m_Quest;

    public override bool Execute()
    {
        Manager.instance.RemoveQuest(m_Quest);

        return true;
    }
}
