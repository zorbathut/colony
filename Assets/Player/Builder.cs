using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class Builder : MonoBehaviour
{
    [SerializeField] Transform m_TargetCube;

    public virtual void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Layers.BuildTarget))
        {
            m_TargetCube.gameObject.SetActive(true);
            m_TargetCube.transform.position = Manager.instance.ClampToGrid(hit.point);
        }
        else
        {
            m_TargetCube.gameObject.SetActive(false);
        }
    }
}
