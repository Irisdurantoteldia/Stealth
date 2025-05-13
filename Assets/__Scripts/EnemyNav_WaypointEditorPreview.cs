#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Aquest component només existeix a l'Editor de Unity i serveix per simular el moviment
// de l'enemic entre waypoints sense necessitat de prémer "Play".

[RequireComponent(typeof(EnemyNav))]
public class EnemyNav_EditorMovementSimulator : MonoBehaviour
{
    [Tooltip("Activar/desactivar la simulació de moviment a l'editor")]
    public bool simulateMovementInEditor = true; // Checkbox que permet activar o desactivar la simulació

    // Variables internes per controlar la simulació
    private int currentWaypointIndex = 0;              // Índex del waypoint actual
    private float simulationProgress = 0f;             // Progrés entre waypoints (de 0 a 1)
    private EnemyNav enemyNav;                         // Referència al component EnemyNav
    private Vector3 simulatedPosition;                 // Posició actual simulada
    private double lastEditorUpdateTime;               // Temps de l'última actualització per controlar deltaTime
    private const float simulationSpeed = 3f;          // Velocitat fixa de simulació

    // Aquest mètode es crida automàticament per Unity per dibuixar gizmos a l’editor
    private void OnDrawGizmos()
    {
        if (!simulateMovementInEditor) return; // Si no està activada la simulació, sortim

        if (enemyNav == null)
            enemyNav = GetComponent<EnemyNav>(); // Assignem el component EnemyNav si encara no el tenim

        // Comprovem que tenim una llista vàlida de waypoints
        if (enemyNav == null || enemyNav.waypoints == null || enemyNav.waypoints.Count < 2)
            return;

        UpdateSimulation(); // Actualitzem la posició simulada

        // Dibuixem una esfera vermella a la posició simulada
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(simulatedPosition, 0.4f);
    }

    // Actualitza el moviment simulat de l’enemic entre waypoints
    private void UpdateSimulation()
    {
        // Calculem el temps transcorregut des de l'última actualització
        double currentTime = EditorApplication.timeSinceStartup;
        double deltaTime = currentTime - lastEditorUpdateTime;
        lastEditorUpdateTime = currentTime;

        // Si ha passat massa temps (p. ex. canvi de finestra), usem un deltaTime per defecte
        if (deltaTime > 1f) deltaTime = 0.016f;

        List<Waypoint> waypoints = enemyNav.waypoints;
        if (waypoints.Count < 2) return;

        // Ens assegurem que l’índex és vàlid
        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1);

        int nextIndex = (currentWaypointIndex + 1) % waypoints.Count; // El següent waypoint
        if (waypoints[currentWaypointIndex] == null || waypoints[nextIndex] == null) return;

        Vector3 currentWaypoint = waypoints[currentWaypointIndex].Position;
        Vector3 nextWaypoint = waypoints[nextIndex].Position;

        // Distància entre els waypoints
        float segmentDistance = Vector3.Distance(currentWaypoint, nextWaypoint);
        if (segmentDistance < 0.001f) segmentDistance = 0.001f;

        // Progrés cap al següent waypoint segons la velocitat i el temps
        float moveAmount = (float)(simulationSpeed * deltaTime) / segmentDistance;
        simulationProgress += moveAmount;

        // Si hem arribat al final del segment, passem al següent
        if (simulationProgress >= 1f)
        {
            simulationProgress -= 1f;
            currentWaypointIndex = nextIndex;
        }

        // Interpolem entre waypoints per obtenir la posició simulada
        simulatedPosition = Vector3.Lerp(currentWaypoint, nextWaypoint, simulationProgress);

        // Forcem la repintada de l’escena per veure els gizmos actualitzats
        SceneView.RepaintAll();
    }
}

[CustomEditor(typeof(EnemyNav_EditorMovementSimulator))]
public class EnemyNav_EditorMovementSimulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Obtenim una referència al component que estem editant
        EnemyNav_EditorMovementSimulator simulator = (EnemyNav_EditorMovementSimulator)target;

        // Mostrem el checkbox per activar/desactivar la simulació
        simulator.simulateMovementInEditor = EditorGUILayout.Toggle("Simular moviment a Editor", simulator.simulateMovementInEditor);

        // Missatge informatiu
        EditorGUILayout.HelpBox("Aquesta opció mostra una simulació visual del moviment entre waypoints al Scene View.", MessageType.Info);
    }
}
#endif
