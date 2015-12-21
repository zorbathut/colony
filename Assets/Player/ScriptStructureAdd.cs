using UnityEngine;
using System.Collections;

public class ScriptStructureAdd : Script
{
    [SerializeField] Structure m_Placeable;

    public override bool Execute()
    {
        Debug.LogFormat("SCRIPT - Add structure {0}", m_Placeable);
        GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().AddStructure(m_Placeable);
        return true;
    }
}
