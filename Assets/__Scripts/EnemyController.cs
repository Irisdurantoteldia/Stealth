using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EnemyPatrol))]
public class EnemyController : Editor
{
    SerializedProperty patrolWaypointsProperty;
    SerializedProperty rotationSpeedProperty;
    SerializedProperty showPathProperty;

    private void OnEnable()
    {
        patrolWaypointsProperty = serializedObject.FindProperty("patrolWaypoints");
        rotationSpeedProperty = serializedObject.FindProperty("rotationSpeed");
        showPathProperty = serializedObject.FindProperty("showPath");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rotationSpeedProperty);
        EditorGUILayout.PropertyField(showPathProperty);

        // Waypoints header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Waypoints", EditorStyles.boldLabel);
        
        // Mostrar la llista de waypoints
        EditorGUILayout.PropertyField(patrolWaypointsProperty, true);

        // Afegir els botons per gestionar waypoints
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Crear Nou Waypoint"))
        {
            CreateNewWaypoint();
        }
        
        if (GUILayout.Button("Numerar Waypoints"))
        {
            NumberWaypoints();
        }
        
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateNewWaypoint()
    {
        EnemyPatrol patrol = (EnemyPatrol)target;
        
        // Crear un GameObject per al nou waypoint
        GameObject waypointObj = new GameObject("Waypoint_" + (patrol.patrolWaypoints.Count + 1));
        
        // Posicionar-lo prop del robot o de l'últim waypoint
        if (patrol.patrolWaypoints.Count > 0 && patrol.patrolWaypoints[patrol.patrolWaypoints.Count - 1] != null)
        {
            waypointObj.transform.position = patrol.patrolWaypoints[patrol.patrolWaypoints.Count - 1].transform.position + 
                                            patrol.patrolWaypoints[patrol.patrolWaypoints.Count - 1].transform.forward * 2.0f;
            waypointObj.transform.rotation = patrol.patrolWaypoints[patrol.patrolWaypoints.Count - 1].transform.rotation;
        }
        else
        {
            waypointObj.transform.position = patrol.transform.position + patrol.transform.forward * 2.0f;
            waypointObj.transform.rotation = patrol.transform.rotation;
        }
        
        // Afegir el component Waypoint
        Waypoint waypoint = waypointObj.AddComponent<Waypoint>();
        
        // Crear el TextMeshPro per al número si és necessari
        GameObject textObj = new GameObject("WaypointNumber");
        textObj.transform.SetParent(waypointObj.transform);
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // Afegir el component TextMeshPro
        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = (patrol.patrolWaypoints.Count + 1).ToString();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.color = Color.yellow;
        
        // Assignar al waypoint
        waypoint.waypointNumberText = textMesh;
        
        // Afegir-lo a la llista de waypoints
        Undo.RecordObject(patrol, "Add Waypoint");
        patrol.patrolWaypoints.Add(waypoint);
        EditorUtility.SetDirty(patrol);
        
        // Seleccionar el nou waypoint
        Selection.activeGameObject = waypointObj;
    }

    private void NumberWaypoints()
    {
        EnemyPatrol patrol = (EnemyPatrol)target;
        
        for (int i = 0; i < patrol.patrolWaypoints.Count; i++)
        {
            if (patrol.patrolWaypoints[i] != null)
            {
                patrol.patrolWaypoints[i].SetWaypointNumber(i + 1);
            }
        }
    }
}