using Rotorz.ReorderableList;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Executor))]
public class ExecutorEditor : Editor
{
    private SerializedProperty m_ScriptsProperty;
    
    private void OnEnable()
    {
        m_ScriptsProperty = serializedObject.FindProperty("m_Scripts");
    }
      
    public override void OnInspectorGUI() {
        serializedObject.Update();

        ReorderableListGUI.Title("Scripts");
        ReorderableListGUI.ListField(m_ScriptsProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
