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
}
