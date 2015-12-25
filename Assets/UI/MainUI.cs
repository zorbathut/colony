using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] PlaceableDisplay m_PlaceableDisplay;
    [SerializeField] RectTransform m_PlaceableAnchor;

    List<PlaceableDisplay> m_PlaceableDisplays = new List<PlaceableDisplay>();

    [SerializeField] QuestDisplay m_QuestDisplay;

    List<QuestDisplay> m_QuestDisplays = new List<QuestDisplay>();

    public virtual void Awake()
    {
        Assert.IsNull(s_Manager);
        s_Manager = this;
    }

    public FadeoutText GetPopupText()
    {
        return m_PopupText;
    }

    public void UpdateStructureList()
    {
        // nuke and pave
        foreach (PlaceableDisplay display in m_PlaceableDisplays)
        {
            Destroy(display.gameObject);
        }
        m_PlaceableDisplays.Clear();

        int index = 0;
        float verticalOffset = m_PlaceableDisplay.GetComponent<RectTransform>().sizeDelta.y;
        foreach (Builder.Placeable placeable in GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<Builder>().GetPlaceables())
        {
            PlaceableDisplay display = UIUtil.RectInstantiate(m_PlaceableDisplay, m_PlaceableAnchor.transform);
            display.Initialize(placeable);
            display.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -verticalOffset) * index;
            m_PlaceableDisplays.Add(display);

            ++index;
        }
    }

    public QuestDisplay AddQuestDisplay()
    {
        QuestDisplay display = UIUtil.RectInstantiate(m_QuestDisplay, this.transform);
        m_QuestDisplays.Add(display);
        RecalculateQuestDisplayPositions();
        return display;
    }

    public void RemoveQuestDisplay(QuestDisplay display)
    {
        m_QuestDisplays.Remove(display);
        Destroy(display.gameObject);
        RecalculateQuestDisplayPositions();
    }

    void RecalculateQuestDisplayPositions()
    {
        float currentPosition = 0;
        foreach (QuestDisplay questDisplay in m_QuestDisplays)
        {
            currentPosition -= questDisplay.GetComponent<RectTransform>().sizeDelta.y;
            questDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, currentPosition);
        }
    }
}
