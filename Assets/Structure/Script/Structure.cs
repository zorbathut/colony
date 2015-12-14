using UnityEngine;
using System.Collections;

public class Structure : MonoBehaviour
{
    [SerializeField] int m_Width = 1;
    [SerializeField] int m_Length = 1;

    public virtual void OnDrawGizmos()
    {
        GizmoUtility.DrawSquare(new Vector3(transform.position.x - (Constants.GridSize / 2) * m_Width, 0f, transform.position.z - (Constants.GridSize / 2) * m_Length), new Vector3(transform.position.x + (Constants.GridSize / 2) * m_Width, 0f, transform.position.z + (Constants.GridSize / 2) * m_Length));
    }
}
