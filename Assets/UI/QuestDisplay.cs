using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;

public class QuestDisplay : MonoBehaviour
{
    [SerializeField] Image m_Pass;
    [SerializeField] Image m_Fail;
    [SerializeField] Text m_Text;

    public void Initialize(string text)
    {
        m_Text.text = text;
        SetPassFlag(false); // sensible default
    }

    public void SetPassFlag(bool flag)
    {
        if (flag)
        {
            m_Pass.gameObject.SetActive(true);
            m_Fail.gameObject.SetActive(false);
        }
        else
        {
            m_Pass.gameObject.SetActive(false);
            m_Fail.gameObject.SetActive(true);
        }
    }
}
