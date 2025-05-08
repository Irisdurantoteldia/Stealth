#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyNav))]
public class EnemyNav_WaypointEditorPreview : MonoBehaviour
{
    [Tooltip("Activar/desactivar la previsualització de la ruta de waypoints")]
    public bool previewWaypointPath = true;

    private void Awake()
    {
        // Amaga tots els objectes amb tag "EditorOnly" si estàs en Play Mode
        if (Application.isPlaying)
        {
            GameObject[] editorOnlyObjects = GameObject.FindGameObjectsWithTag("EditorOnly");
            foreach (GameObject obj in editorOnlyObjects)
            {
                obj.SetActive(false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!previewWaypointPath) return;

        EnemyNav enemyNav = GetComponent<EnemyNav>();
        if (enemyNav == null || enemyNav.waypoints == null || enemyNav.waypoints.Count == 0) return;

        Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.5f);

        List<Waypoint> waypoints = enemyNav.waypoints;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].Position, 0.3f);

            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].Position, waypoints[nextIndex].Position);

                Vector3 direction = (waypoints[nextIndex].Position - waypoints[i].Position).normalized;
                Vector3 midPoint = Vector3.Lerp(waypoints[i].Position, waypoints[nextIndex].Position, 0.5f);

                DrawArrow(midPoint, direction, 0.5f, 20f);
            }
        }
    }

    private void DrawArrow(Vector3 position, Vector3 direction, float length, float arrowHeadAngle)
    {
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;

        Gizmos.DrawRay(position, right * length);
        Gizmos.DrawRay(position, left * length);
    }
}
#endif
