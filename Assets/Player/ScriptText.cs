using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class ScriptText : Script
{
    [SerializeField, Multiline] string m_Text;

    [SerializeField] float m_FadeIn = 2f;
    [SerializeField] float m_FadeOut = 1f;

    enum Phase
    {
        FadeIn,
        Wait,
        FadeOut,
        Done,
    };
    Phase m_Phase = Phase.FadeIn;

    float m_Opacity = 0f;

    public override bool Execute()
    {
        switch (m_Phase)
        {
            case Phase.FadeIn:
                m_Opacity = Mathf.Min(m_Opacity + Time.deltaTime / m_FadeIn, 1f);
                MainUI.instance.SetTextOverlay(m_Text, m_Opacity);
                if (m_Opacity >= 1f)
                {
                    m_Phase = Phase.Wait;
                }
                return false;

            case Phase.Wait:
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    m_Phase = Phase.FadeOut;
                }
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
