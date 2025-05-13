using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Temps que l'enemic esperarà en aquest punt abans de continuar")]
    public float waitTime = 2.0f; 

    /// <summary>
    /// Obté la posició del punt de pas
    /// </summary>
    public Vector3 Position => transform.position; // Retorna la posició actual del waypoint

    /// <summary>
    /// Obté la rotació del punt de pas
    /// </summary>
    public Quaternion Rotation => transform.rotation; // Retorna la rotació actual del waypoint

    /// <summary>
    /// Obté la direcció endavant del punt de pas
    /// </summary>
    public Vector3 Forward => transform.forward; // Retorna la direcció en la qual el waypoint està orientat

    private void OnDrawGizmos()
    {
        // Dibuixa un cub per representar el waypoint a l'escena de Unity (només en l'editor)
        Gizmos.color = Color.blue; // Configurem el color del cub a blau
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f)); // Dibuixem un cub de mida 0.5 unitats

        // Dibuixa una fletxa per mostrar la direcció en què el waypoint està orientat
        Gizmos.color = Color.green; // Configurem el color de la fletxa a verd
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f); // Dibuixem una línia en la direcció endavant del waypoint (ampliada a 1.5 unitats)
    }
}
