using UnityEngine;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] int m_Width = 1;
    [SerializeField] int m_Length = 1;

    [SerializeField] bool m_Walled = false;
    [SerializeField] bool m_DoorCreator = false;

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
