using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [SerializeField] Transform m_TargetCube;
    [SerializeField] List<Structure> m_Placeable;

    public virtual void Update()
    {
        // Test placement
        // Just yanking the current cursor state out of the target cube; ugly, but viable
        if (m_TargetCube.gameObject.activeSelf && Input.GetMouseButtonDown(0) && m_Placeable.Count != 0)
        {
            // Place that thing!
            if (Manager.instance.PlaceAttempt(m_Placeable[0], m_TargetCube.transform.position))
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
        Vector3 position = new Vector3();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1 << Layers.BuildTarget))
        {
            m_TargetCube.gameObject.SetActive(true);

            m_TargetCube.localScale = new Vector3(nextStructure.GetWidth(), 1, nextStructure.GetLength()) * 3; // magic number to make the cube look visually good

            position = Manager.instance.ClampToGrid(hit.point);

            // shift cube to be centered relative to the object's width and length
            position.x = position.x + (nextStructure.GetWidth() - 1) * Constants.GridSize / 2;
            position.z = position.z + (nextStructure.GetLength() - 1) * Constants.GridSize / 2;

            m_TargetCube.transform.position = position;
        }
        else
        {
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
