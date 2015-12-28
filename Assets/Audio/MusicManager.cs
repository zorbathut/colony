using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    /////////////////////////////////////////////
    // SINGLETON
    //

    static MusicManager s_Manager = null;
    public static MusicManager instance
    {
        get
        {
            return s_Manager;
        }
    }

    /////////////////////////////////////////////
    // EVERYTHING ELSE
    //

    [SerializeField] AudioSource m_Active;
    [SerializeField] AudioSource m_Fadeout;

    [SerializeField] float m_FadeTime;

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    public virtual void Update()
    {
        m_Active.volume = Mathf.Min(m_Active.volume + Time.deltaTime / m_FadeTime, 1f);
        m_Fadeout.volume = Mathf.Max(m_Fadeout.volume - Time.deltaTime / m_FadeTime, 0f);
        if (m_Fadeout.volume == 0)
        {
            m_Fadeout.Stop();
        }
    }

    public void ChangeMusic(AudioClip music)
    {
        // We kill the current fadeout entirely; this will cause an audio pop if we're still fading out, but that should never happen in context
        Assert.IsFalse(m_Fadeout.isPlaying);
        m_Fadeout.Stop();

        // Swap the two channels, so we can fade out the active
        {
            AudioSource temp = m_Active;
            m_Active = m_Fadeout;
            m_Fadeout = temp;
        }

        // Setup the new active
        m_Active.volume = 0f;
        m_Active.clip = music;
        m_Active.Play();
    }
}
