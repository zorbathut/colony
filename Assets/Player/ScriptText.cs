using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class ScriptText : Script
{
    [SerializeField, Multiline] string m_Text;

    float m_Delay = 1f;
    float m_FadeIn = 2f;
    float m_FadeOut = 0.5f;

    enum Phase
    {
        Delay,
        FadeIn,
        Wait,
        FadeOut,
        Done,
    };
    Phase m_Phase = Phase.Delay;

    float m_Opacity = 0f;

    public virtual void Update()
    {
        // this is pretty awful because this is run each tick for each Text, but the game's almost done so I'm not gonna worry about it
        if ((m_Phase == Phase.Wait || (m_Phase == Phase.FadeIn && m_Opacity > 0.25f)) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0)))
        {
            m_Phase = Phase.FadeOut;
        }

        if (m_Phase == Phase.Wait && Input.GetMouseButtonDown(0))
        {
            m_Phase = Phase.FadeOut;
        }
    }

    public override bool Execute()
    {
        if (ScriptQuestEvaluate.ConsumeDebugOverride())
        {
            MainUI.instance.SetTextOverlay(m_Text, 0f);
            return true;
        }

        switch (m_Phase)
        {
            case Phase.Delay:
                m_Delay -= Time.deltaTime;
                if (m_Delay <= 0)
                {
                    m_Phase = Phase.FadeIn;
                }
                return false;

            case Phase.FadeIn:
                m_Opacity = Mathf.Min(m_Opacity + Time.deltaTime / m_FadeIn, 1f);
                MainUI.instance.SetTextOverlay(m_Text, m_Opacity);
                if (m_Opacity >= 1f)
                {
                    m_Phase = Phase.Wait;
                }
                return false;

            case Phase.Wait:
                // transition handled in Update()
                return false;

            case Phase.FadeOut:
                m_Opacity = Mathf.Max(m_Opacity - Time.deltaTime / m_FadeOut, 0f);
                MainUI.instance.SetTextOverlay(m_Text, m_Opacity);
                if (m_Opacity <= 0f)
                {
                    m_Phase = Phase.Done;
                }
                return false;

            case Phase.Done:
                return true;

            default:
                // uh what
                Assert.IsTrue(false);
                return true;
        }
    }
}
