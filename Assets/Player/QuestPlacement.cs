using UnityEngine;
using System.Collections;

public class QuestPlacement : Quest
{
    [SerializeField] Structure m_Structure;
    [SerializeField] bool m_Removed = false;

    public override string GetTextual()
    {
        if (!m_Removed)
        {
            return string.Format("Place {0}", m_Structure.name);
        }
        else
        {
            return string.Format("Remove {0}", m_Structure.name);
        }
    }

    public override bool IsComplete()
    {
        return (Manager.instance.GetStructureOfType(m_Structure) == null) == m_Removed;
    }
}
