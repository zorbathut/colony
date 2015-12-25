using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] int m_Width = 1;
    [SerializeField] int m_Length = 1;

    [SerializeField] bool m_Walled = false;
    [SerializeField] bool m_DoorCreator = false;

    [SerializeField, HideInInspector] Structure m_Template = null;
    [SerializeField, HideInInspector] IntVector2 m_Origin;

    public void Initialize(Structure template, IntVector2 origin)
    {
        Assert.IsNull(m_Template);
        Assert.IsNotNull(template);

        m_Template = template;
        m_Origin = origin;

        Assert.IsTrue(Manager.instance.GetStructureFromIndex(origin) == this);
    }

    public Structure GetTemplate()
    {
        return m_Template;
    }

    public IntVector2 GetOrigin()
    {
        return m_Origin;
    }

    public virtual void OnDrawGizmos()
    {
        GizmoUtility.DrawSquare(new Vector3(transform.position.x - (Constants.GridSize / 2) * m_Width, 0f, transform.position.z - (Constants.GridSize / 2) * m_Length), new Vector3(transform.position.x + (Constants.GridSize / 2) * m_Width, 0f, transform.position.z + (Constants.GridSize / 2) * m_Length));
    }

    public bool GetWalled()
    {
        return m_Walled;
    }

    public bool GetDoorCreator()
    {
        return m_DoorCreator;
    }

    public int GetWidth()
    {
        return m_Width;
    }

    public int GetLength()
    {
        return m_Length;
    }
}
