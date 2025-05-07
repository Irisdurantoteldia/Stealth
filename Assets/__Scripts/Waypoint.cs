using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Temps que l'enemic esperarà en aquest punt abans de continuar")]
    public float waitTime = 2.0f;

    /// <summary>
    /// Obté la posició del punt de pas
    /// </summary>
    public Vector3 Position => transform.position;

    /// <summary>
    /// Obté la rotació del punt de pas
    /// </summary>
    public Quaternion Rotation => transform.rotation;

    /// <summary>
    /// Obté la direcció endavant del punt de pas
    /// </summary>
    public Vector3 Forward => transform.forward;

    private void OnDrawGizmos()
    {
        // Dibuixa un cub per representar el waypoint
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));

        // Dibuixa una fletxa per mostrar la direcció
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}