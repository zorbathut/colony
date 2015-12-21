using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
    [System.Serializable]
    public class Placeable
    {
        public Structure template;
        public bool infinite;
        public int remaining;
    }

    [SerializeField] Transform m_PlacementCube;
    [SerializeField] Transform m_DestructionCube;

    [SerializeField, HideInInspector] List<Placeable> m_Placeable = new List<Placeable>();
    [SerializeField, HideInInspector] int m_PlaceableIndex;

    Vector3 m_TargetPosition;
    bool m_TargetPositionValid = false;

    public virtual void Update()
    {
        string errorString = null;

        // Change index
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            --m_PlaceableIndex;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ++m_PlaceableIndex;
        }
        m_PlaceableIndex = Mathf.Clamp(m_PlaceableIndex, 0, m_Placeable.Count - 1); // happens every frame just in case we remove things from placeable

        // Test removal
        if (Input.GetMouseButtonDown(1))
        {
            if (!m_TargetPositionValid)
            {
                errorString = "You can't remove the sky.";
            }
            else if (!Manager.instance.GetObject(m_TargetPosition))
            {
                errorString = "Nothing is there to remove.";
            }
            else
            {
                Structure removed = Manager.instance.Remove(m_TargetPosition, out errorString);
                if (removed)
                {
                    // Success!
                    Placeable placeable = FindPlaceableByTemplate(removed);
                    Assert.IsNotNull(placeable);
                    if (placeable != null)
                    {
                        ++placeable.remaining;
                    }
                }
            }
        }

        // Test placement
        if (Input.GetMouseButtonDown(0))
        {
            Placeable placeable = GetCurrentPlaceable();
            if (!m_TargetPositionValid)
            {
                errorString = "Only the gods can build in the sky.";
            }
            else if (placeable == null)
            {
                errorString = "You don't have anything left to place.";
            }
            else if (!placeable.infinite && placeable.remaining <= 0)
            {
                errorString = "You don't have any of those left to place.";
            }
            else if (Manager.instance.GetObject(m_TargetPosition))
            {
                errorString = "A building must not be placed on another building.";
            }
            else if (Manager.instance.PlaceAttempt(placeable.template, m_TargetPosition, out errorString))
            {
                // Success!
                --placeable.remaining;
            }
        }
        
        if (errorString != null)
        {
            MainUI.instance.GetPopupText().DisplayText(errorString, Color.white);
            Debug.Log(errorString);
        }
    }

    public virtual void FixedUpdate()
    {
        // Clear to defaults
        m_TargetPositionValid = false;
        m_PlacementCube.gameObject.SetActive(false);
        m_DestructionCube.gameObject.SetActive(false);

        // Move highlight box, figure out what the user's pointing at
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, 1 << Layers.BuildTarget))
        {
            // Stash these away; we can't retrieve them from the cube later on because we have to munge the cube data
            m_TargetPositionValid = true;
            m_TargetPosition = Manager.instance.ClampToGrid(hit.point);

            // Figure out what kind of display cube we should be using
            Structure targetStructure = Manager.instance.GetObject(m_TargetPosition);
            Structure nextStructure = GetCurrentPlaceable() != null ? GetCurrentPlaceable().template : null;

            if (targetStructure)
            {
                // removal
                m_DestructionCube.gameObject.SetActive(true);

                m_DestructionCube.localScale = new Vector3(targetStructure.GetWidth(), 1, targetStructure.GetLength()) * 3; // magic number to make the cube look visually good

                m_DestructionCube.transform.position = targetStructure.transform.position;
            }
            else if (nextStructure)
            {
                // placement
                m_PlacementCube.gameObject.SetActive(true);

                m_PlacementCube.localScale = new Vector3(nextStructure.GetWidth(), 1, nextStructure.GetLength()) * 3; // magic number to make the cube look visually good

                Vector3 cubePosition = m_TargetPosition;

                // shift cube to be centered relative to the object's width and length
                cubePosition.x = cubePosition.x + (nextStructure.GetWidth() - 1) * Constants.GridSize / 2;
                cubePosition.z = cubePosition.z + (nextStructure.GetLength() - 1) * Constants.GridSize / 2;

                m_PlacementCube.transform.position = cubePosition;
            }
        }
    }

    public Placeable GetCurrentPlaceable()
    {
        if (m_Placeable.Count == 0)
        {
            return null;
        }

        if (m_PlaceableIndex < 0 || m_PlaceableIndex >= m_Placeable.Count)
        {
            return null;
        }

        return m_Placeable[m_PlaceableIndex];
    }

    public List<Placeable> GetPlaceables()
    {
        return m_Placeable;
    }

    public Placeable FindPlaceableByTemplate(Structure structure)
    {
        foreach (Placeable placeable in m_Placeable)
        {
            if (placeable.template == structure)
            {
                return placeable;
            }
        }

        return null;
    }

    public int GetPlaceableIndex()
    {
        return m_PlaceableIndex;
    }

    public void AddStructure(Structure structure)
    {
        Placeable placeable = new Placeable();
        placeable.template = structure;
        placeable.infinite = true;
        placeable.remaining = 0;
        m_Placeable.Add(placeable);
    }
}
