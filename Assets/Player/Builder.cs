using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [SerializeField] Transform m_TargetCube;
    [SerializeField] List<Structure> m_Placeable;

    Vector3 m_TargetPosition;
    bool m_TargetPositionValid = false;

    public virtual void Update()
    {
        // Test placement
        if (m_TargetPositionValid && Input.GetMouseButtonDown(0) && m_Placeable.Count != 0)
        {
            // Place that thing!
            if (Manager.instance.PlaceAttempt(m_Placeable[0], m_TargetPosition))
            {
                m_Placeable.RemoveAt(0);
            }
        }
    }

    public virtual void FixedUpdate()
    {
        Structure nextStructure = GetNextStructure();

        if (!nextStructure)
        {
            // no next structure, no box
            m_TargetCube.gameObject.SetActive(false);
            return;
        }

        // Move highlight box, figure out what the user's pointing at
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << Layers.BuildTarget))
        {
            // Stash these away; we can't retrieve them from the cube later on because we have to munge the cube data
            m_TargetPositionValid = true;
            m_TargetPosition = Manager.instance.ClampToGrid(hit.point);

            m_TargetCube.gameObject.SetActive(true);

            m_TargetCube.localScale = new Vector3(nextStructure.GetWidth(), 1, nextStructure.GetLength()) * 3; // magic number to make the cube look visually good

            Vector3 cubePosition = m_TargetPosition;

            // shift cube to be centered relative to the object's width and length
            cubePosition.x = cubePosition.x + (nextStructure.GetWidth() - 1) * Constants.GridSize / 2;
            cubePosition.z = cubePosition.z + (nextStructure.GetLength() - 1) * Constants.GridSize / 2;

            m_TargetCube.transform.position = cubePosition;
        }
        else
        {
            m_TargetPositionValid = false;
            m_TargetCube.gameObject.SetActive(false);
        }
    }

    Structure GetNextStructure()
    {
        if (m_Placeable.Count != 0)
        {
            return m_Placeable[0];
        }
        else
        {
            return null;
        }
    }
}
