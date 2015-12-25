﻿using UnityEngine;
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
        public bool active;
    }

    [SerializeField] Transform m_PlacementCube;
    [SerializeField] Transform m_DestructionCube;

    [SerializeField, HideInInspector] List<Placeable> m_Placeables = new List<Placeable>();
    [SerializeField, HideInInspector] int m_PlaceablesIndex;

    Vector3 m_TargetPosition;
    bool m_TargetPositionValid = false;

    public virtual void Update()
    {
        string errorString = null;

        // Change index
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            --m_PlaceablesIndex;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ++m_PlaceablesIndex;
        }
        m_PlaceablesIndex = Mathf.Clamp(m_PlaceablesIndex, 0, m_Placeables.Count - 1); // happens every frame just in case we remove things from placeable

        for (int i = 0; i < m_Placeables.Count; ++i)
        {
            m_Placeables[i].active = (i == m_PlaceablesIndex);
        }

        // Test removal
        if (Input.GetMouseButtonDown(1))
        {
            if (!m_TargetPositionValid)
            {
                errorString = "You can't remove the sky.";
            }
            else if (!Manager.instance.GetStructureFromCoordinate(m_TargetPosition))
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
            else if (Manager.instance.GetStructureFromCoordinate(m_TargetPosition))
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
            Structure targetStructure = Manager.instance.GetStructureFromCoordinate(m_TargetPosition);
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
        if (m_Placeables.Count == 0)
        {
            return null;
        }

        if (m_PlaceablesIndex < 0 || m_PlaceablesIndex >= m_Placeables.Count)
        {
            return null;
        }

        return m_Placeables[m_PlaceablesIndex];
    }

    public List<Placeable> GetPlaceables()
    {
        return m_Placeables;
    }

    public Placeable FindPlaceableByTemplate(Structure structure)
    {
        foreach (Placeable placeable in m_Placeables)
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
        return m_PlaceablesIndex;
    }

    public void AddStructure(Structure structure, bool infinite)
    {
        Placeable placeable = new Placeable();
        placeable.template = structure;
        placeable.infinite = infinite;
        placeable.remaining = 1;    // safe default if we're infinite
        m_Placeables.Add(placeable);

        GameObject.FindGameObjectWithTag(Tags.UI).GetComponent<MainUI>().UpdateStructureList();
    }
}
