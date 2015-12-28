using UnityEngine;
using System.Collections;

public class ScriptMusic : Script
{
    [SerializeField] AudioClip m_Music;
    
    public override bool Execute()
    {
        MusicManager.instance.ChangeMusic(m_Music);

        return true;
    }
}
