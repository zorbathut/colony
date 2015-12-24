using UnityEngine;
using System.Collections;

public class ScriptStructureAdd : Script
{
    [SerializeField] Structure m_Placeable;
    [SerializeField] bool m_Infinite;

    public override bool Execute()
    {
        Debug.LogFormat("SCRIPT - Add structure {0}", m_Placeable);
        GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().AddStructure(m_Placeable, m_Infinite);
        return true;
    }
}
