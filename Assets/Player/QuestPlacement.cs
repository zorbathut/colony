using UnityEngine;
using System.Collections;

public class QuestPlacement : Quest
{
    [SerializeField] Structure m_Structure;

    public override string GetTextual()
    {
        return string.Format("Place {0}", m_Structure.name);
    }

    public override bool IsComplete()
    {
        return Manager.instance.GetStructureOfType(m_Structure) != null;
    }
}
