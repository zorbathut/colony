﻿using UnityEngine;
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
            Manager.instance.PlaceAttempt(m_Placeable[0], m_TargetCube.transform.position);
            m_Placeable.RemoveAt(0);
        }
    }

    public virtual void FixedUpdate()
    {
        // Move highlight box, figure out what the user's pointing at
        RaycastHit hit;
        Vector3 position = new Vector3();
        bool hasPosition = false;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Layers.BuildTarget))
        {
            m_TargetCube.gameObject.SetActive(true);
            position = Manager.instance.ClampToGrid(hit.point);
            hasPosition = true;
            m_TargetCube.transform.position = position;
        }
        else
        {
            m_TargetCube.gameObject.SetActive(false);
        }
    }
}
