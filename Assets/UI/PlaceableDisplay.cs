using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;

public class PlaceableDisplay : MonoBehaviour
{
    [SerializeField] Text m_Selector;
    [SerializeField] Text m_Name;
    [SerializeField] Text m_Quantity;

    Builder.Placeable m_Placeable;

    public void Initialize(Builder.Placeable placeable)
    {
        Assert.IsNull(m_Placeable);
        Assert.IsNotNull(placeable);

        m_Placeable = placeable;

        m_Name.text = m_Placeable.template.name;
    }

    public virtual void Update()
    {
        m_Selector.gameObject.SetActive(m_Placeable.active);
        if (m_Placeable.infinite)
        {
            m_Quantity.text = "(infinite)";
        }
        else
        {
            m_Quantity.text = string.Format("{0} remaining", m_Placeable.remaining);
        }
    }
}
