#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyNav))]
public class EnemyNav_WaypointEditorPreview : MonoBehaviour
{
    [Tooltip("Activar/desactivar la previsualització de la ruta de waypoints")]
    public bool previewWaypointPath = true;

    private void OnDrawGizmos()
    {
        if (!previewWaypointPath) return;

        EnemyNav enemyNav = GetComponent<EnemyNav>();
        if (enemyNav == null || enemyNav.waypoints == null || enemyNav.waypoints.Count == 0) return;

        // Dibuixar línies entre els waypoints en mode editor
        Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.5f);
        
        List<Waypoint> waypoints = enemyNav.waypoints;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            
            // Dibuixar una esfera al waypoint actual
            Gizmos.DrawSphere(waypoints[i].Position, 0.3f);
            
            // Dibuixar una línia des d'aquest waypoint al següent
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].Position, waypoints[nextIndex].Position);
                
                // Dibuixar fletxes direccionals per mostrar el sentit de la ruta
                Vector3 direction = (waypoints[nextIndex].Position - waypoints[i].Position).normalized;
                Vector3 midPoint = Vector3.Lerp(waypoints[i].Position, waypoints[nextIndex].Position, 0.5f);
                
                // Dibuixar una fletxa al punt mig
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