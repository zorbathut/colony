using UnityEngine;
using System.Collections;

public class ScriptStructureRemove : Script
{
    [SerializeField] Structure m_Placeable;

    public override bool Execute()
    {
        Debug.LogFormat("SCRIPT - Remove structure {0}", m_Placeable);
        GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().RemoveStructure(m_Placeable);
        return true;
    }
}
