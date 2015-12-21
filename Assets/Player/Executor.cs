using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Executor : MonoBehaviour
{
    [SerializeField] List<Script> m_Scripts;
    int m_ExecutionIndex = 0;

    public virtual void FixedUpdate()
    {
        while (m_ExecutionIndex < m_Scripts.Count)
        {
            if (!m_Scripts[m_ExecutionIndex].Execute())
            {
                // end continuing execution here
                return;
            }

            ++m_ExecutionIndex;
        }
    }
}
