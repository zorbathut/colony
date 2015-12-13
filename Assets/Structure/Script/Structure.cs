using UnityEngine;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] int m_Width = 1;
    [SerializeField] int m_Length = 1;

    public virtual void OnDrawGizmos()
    {
        GizmoUtility.DrawSquare(new Vector3(transform.position.x - 1.5f * m_Width, 0f, transform.position.z - 1.5f * m_Length), new Vector3(transform.position.x + 1.5f * m_Width, 0f, transform.position.z + 1.5f * m_Length));
    }
}
