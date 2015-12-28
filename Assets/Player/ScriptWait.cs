using UnityEngine;
using System.Collections;

public class ScriptWait : Script
{
    [SerializeField] float m_Duration;
    
    public override bool Execute()
    {
        // this assumes we get called once per "update", whether that be Update or FixedUpdate, but right now we do, so, cool

        m_Duration -= Time.deltaTime;

        return m_Duration <= 0;
    }
}
