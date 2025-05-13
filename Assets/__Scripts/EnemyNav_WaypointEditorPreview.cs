#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyNav))]
public class EnemyNav_EditorMovementSimulator : MonoBehaviour
{
    [Tooltip("Activar/desactivar la simulació de moviment a l'editor")]
    public bool simulateMovementInEditor = true;

    [Tooltip("Velocitat de la simulació en l'editor")]
    [Range(0.1f, 10f)]
    public float simulationSpeed = 2f;

    [Tooltip("Prefab del robot a mostrar (si està buit, s'utilitzarà un cercle)")]
    public GameObject robotPrefab;

    [Tooltip("Mostrar estela del recorregut")]
    public bool showTrail = true;

    [Tooltip("Color de l'estela")]
    public Color trailColor = new Color(0.2f, 0.9f, 0.2f, 0.3f);

    // Variables internes per la simulació
    private int currentWaypointIndex = 0;
    private float simulationProgress = 0f;
    private EnemyNav enemyNav;
    private Vector3 simulatedPosition;
    private Vector3 previousPosition;
    private Quaternion simulatedRotation = Quaternion.identity;
    private List<Vector3> trailPositions = new List<Vector3>();
    private double lastEditorUpdateTime;
    private GameObject robotInstance;

    // Mètode que s'executa quan es dibuixen els gizmos a l'editor
    private void OnDrawGizmos()
    {
        if (!simulateMovementInEditor) return;

        // Inicialitzar si és necessari
        if (enemyNav == null)
            enemyNav = GetComponent<EnemyNav>();

        if (enemyNav == null || enemyNav.waypoints == null || enemyNav.waypoints.Count < 2)
            return;

        // Actualitzar la simulació
        UpdateSimulation();

        // Dibuixar el robot o el marcador
        DrawRobotOrMarker();

        // Dibuixar l'estela si està activada
        if (showTrail && trailPositions.Count > 1)
            DrawTrail();
    }

    // Actualitza la posició simulada de l'enemic
    private void UpdateSimulation()
    {
        // Calcular el temps transcorregut des de l'última actualització
        double currentTime = EditorApplication.timeSinceStartup;
        double deltaTime = currentTime - lastEditorUpdateTime;
        lastEditorUpdateTime = currentTime;

        // Si és la primera actualització, inicialitzar
        if (deltaTime > 1f) deltaTime = 0.016f;

        List<Waypoint> waypoints = enemyNav.waypoints;
        if (waypoints.Count < 2) return;

        // Assegurar-nos que els índexs són vàlids
        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1);

        // Obtenir waypoint actual i següent
        int nextIndex = (currentWaypointIndex + 1) % waypoints.Count;
        if (waypoints[currentWaypointIndex] == null || waypoints[nextIndex] == null) return;

        Vector3 currentWaypoint = waypoints[currentWaypointIndex].Position;
        Vector3 nextWaypoint = waypoints[nextIndex].Position;

        // Calcular la distància entre els waypoints
        float segmentDistance = Vector3.Distance(currentWaypoint, nextWaypoint);
        if (segmentDistance < 0.001f) segmentDistance = 0.001f;

        // Actualitzar la progressió
        float moveAmount = (float)(simulationSpeed * deltaTime) / segmentDistance;
        simulationProgress += moveAmount;

        // Si hem arribat al següent waypoint
        if (simulationProgress >= 1f)
        {
            simulationProgress -= 1f;
            currentWaypointIndex = nextIndex;
            
            // Actualitzar per la següent iteració
            nextIndex = (currentWaypointIndex + 1) % waypoints.Count;
            currentWaypoint = waypoints[currentWaypointIndex].Position;
            nextWaypoint = waypoints[nextIndex].Position;
        }

        // Calcular la posició actual interpolada
        previousPosition = simulatedPosition;
        simulatedPosition = Vector3.Lerp(currentWaypoint, nextWaypoint, simulationProgress);

        // Calcular la rotació per mirar cap a la direcció del moviment
        if (previousPosition != Vector3.zero && Vector3.Distance(simulatedPosition, previousPosition) > 0.001f)
        {
            Vector3 direction = (simulatedPosition - previousPosition).normalized;
            simulatedRotation = Quaternion.LookRotation(direction);
        }

        // Afegir posició a l'estela
        if (showTrail && (trailPositions.Count == 0 || Vector3.Distance(simulatedPosition, trailPositions[trailPositions.Count - 1]) > 0.3f))
        {
            trailPositions.Add(simulatedPosition);
            if (trailPositions.Count > 20) // Limitar la longitud de l'estela
                trailPositions.RemoveAt(0);
        }

        // Forçar la repintada de l'escena
        SceneView.RepaintAll();
    }

    // Dibuixa el robot o un marcador si no hi ha robot
    private void DrawRobotOrMarker()
    {
        if (robotPrefab != null)
        {
            // Si tenim un prefab de robot, el mostrem a la posició simulada
            Matrix4x4 originalMatrix = Gizmos.matrix;

            // Aplicar transformació a la posició i rotació simulades
            Gizmos.matrix = Matrix4x4.TRS(simulatedPosition, simulatedRotation, Vector3.one);
            
            // Dibuixar un wireframe que representa el robot
            // Això utilitza la mesh del prefab per dibuixar-la com a gizmo
            MeshFilter[] meshFilters = robotPrefab.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    // Dibuixar cada mesh del robot
                    Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.7f);  // Color blavós
                    Gizmos.DrawWireMesh(meshFilter.sharedMesh, 
                                       meshFilter.transform.localPosition, 
                                       meshFilter.transform.localRotation, 
                                       meshFilter.transform.localScale);
                }
            }

            // Restaurar la matriu original
            Gizmos.matrix = originalMatrix;
            
            // Si no hi ha meshes al prefab, dibuixar una forma bàsica
            if (meshFilters.Length == 0)
            {
                Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.7f);
                Gizmos.DrawWireCube(simulatedPosition, new Vector3(1f, 2f, 1f));
            }
        }
        else
        {
            // Si no tenim un prefab de robot, dibuixem un simple cercle
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f); // Vermell
            Gizmos.DrawSphere(simulatedPosition, 0.5f);

            // Dibuixar una fletxa per indicar la direcció
            if (previousPosition != Vector3.zero && Vector3.Distance(simulatedPosition, previousPosition) > 0.001f)
            {
                Vector3 direction = (simulatedPosition - previousPosition).normalized;
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 30, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -30, 0) * Vector3.forward;

                Gizmos.DrawRay(simulatedPosition, direction * 0.5f);
                Gizmos.DrawRay(simulatedPosition, right * 0.25f);
                Gizmos.DrawRay(simulatedPosition, left * 0.25f);
            }
        }
    }

    // Dibuixa l'estela que segueix al moviment
    private void DrawTrail()
    {
        for (int i = 0; i < trailPositions.Count - 1; i++)
        {
            // Fer que l'opacitat disminueixi amb la distància
            float alpha = trailColor.a * (i / (float)trailPositions.Count);
            Gizmos.color = new Color(trailColor.r, trailColor.g, trailColor.b, alpha);
            
            // Connectar els punts de l'estela amb línies
            Gizmos.DrawLine(trailPositions[i], trailPositions[i + 1]);
            
            // Dibuixar petits cercles al recorregut
            Gizmos.DrawSphere(trailPositions[i], 0.1f);
        }
    }
}

// Editor personalitzat per controlar la simulació
[CustomEditor(typeof(EnemyNav_EditorMovementSimulator))]
public class EnemyNav_EditorMovementSimulatorEditor : Editor
{
    private SerializedProperty simulateMovementInEditor;
    private SerializedProperty simulationSpeed;
    private SerializedProperty robotPrefab;
    private SerializedProperty showTrail;
    private SerializedProperty trailColor;

    private void OnEnable()
    {
        simulateMovementInEditor = serializedObject.FindProperty("simulateMovementInEditor");
        simulationSpeed = serializedObject.FindProperty("simulationSpeed");
        robotPrefab = serializedObject.FindProperty("robotPrefab");
        showTrail = serializedObject.FindProperty("showTrail");
        trailColor = serializedObject.FindProperty("trailColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(simulateMovementInEditor);
        EditorGUILayout.PropertyField(simulationSpeed);
        
        EditorGUILayout.PropertyField(robotPrefab);
        EditorGUILayout.HelpBox("Arrossega aquí el prefab del teu robot per visualitzar-lo a l'editor", MessageType.Info);
        
        EditorGUILayout.PropertyField(showTrail);
        EditorGUILayout.PropertyField(trailColor);

        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Controls de Simulació", EditorStyles.boldLabel);
        
        // Botons per controlar la simulació
        EditorGUILayout.BeginHorizontal();
        
        EnemyNav_EditorMovementSimulator simulator = (EnemyNav_EditorMovementSimulator)target;
        
        if (GUILayout.Button("Reiniciar"))
        {
            // Reiniciar la simulació
            simulator.GetType().GetField("currentWaypointIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simulator, 0);
            simulator.GetType().GetField("simulationProgress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simulator, 0f);
            simulator.GetType().GetField("trailPositions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(simulator, new List<Vector3>());
            
            // Forçar repintat
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox("Aquest component simula el moviment de l'enemic a través dels waypoints directament a l'editor, sense necessitat de prémer Play.", MessageType.Info);
    }
}
#endif