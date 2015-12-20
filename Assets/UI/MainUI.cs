using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class MainUI : MonoBehaviour
{
    // singleton
    static MainUI s_Manager = null;
    public static MainUI instance
    {
        get
        {
            return s_Manager;
        }
    }

    [SerializeField] FadeoutText m_PopupText;

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    public FadeoutText GetPopupText()
    {
        return m_PopupText;
    }
}
