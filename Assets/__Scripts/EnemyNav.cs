using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNav : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Llista de punts de pas per patrullar")]
    public List<Waypoint> waypoints = new List<Waypoint>();
    
    [Tooltip("Velocitat de moviment")]
    public float speed = 4.0f;
    
    [Tooltip("Velocitat de rotació")]
    public float angularSpeed = 90.0f;

    [Header("State")]
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private int targetWaypointIndex = 0;
    
    // Components
    private NavMeshAgent agent;

    // Estat intern
    private float waitTimer = 0f;
    private enum NavState { RotatingBeforeMoving, Moving, RotatingAtWaypoint, Waiting }
    private NavState currentState = NavState.RotatingBeforeMoving;

    [Header("Debug")]
    [SerializeField]
    private bool drawGizmos = true;
    [SerializeField]
    private string currentStateDebug = "";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Configurar l'agent
        agent.speed = speed;
        agent.angularSpeed = angularSpeed;
        agent.updateRotation = false; // Important: Desactivem la rotació automàtica
        
        // Iniciar el patrullatge si hi ha waypoints
        if (waypoints.Count > 0)
        {
            currentWaypointIndex = 0;
            targetWaypointIndex = 0;
            currentState = NavState.RotatingBeforeMoving;
        }
    }

    private void Update()
    {
        if (waypoints.Count == 0) return;

        // Actualitzar el debug de l'estat
        currentStateDebug = currentState.ToString();

        switch (currentState)
        {
            case NavState.RotatingBeforeMoving:
                UpdateRotationBeforeMoving();
                break;
            case NavState.Moving:
                UpdateMovement();
                break;
            case NavState.RotatingAtWaypoint:
                UpdateRotationAtWaypoint();
                break;
            case NavState.Waiting:
                UpdateWaiting();
                break;
        }
    }

    private void UpdateRotationBeforeMoving()
    {
        // Calculem la direcció cap al waypoint objectiu
        Vector3 targetDirection = waypoints[targetWaypointIndex].Position - transform.position;
        targetDirection.y = 0; // Assegurem que la rotació sigui només en l'eix Y

        if (targetDirection.magnitude < 0.1f)
        {
            // Si estem massa a prop, passem directament a la rotació al waypoint
            currentState = NavState.RotatingAtWaypoint;
            return;
        }

        // Calculem la rotació desitjada (mirant cap al waypoint)
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Rotem gradualment cap a l'orientació desitjada
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
            angularSpeed * Time.deltaTime);
        
        // Comprovem si ja estem orientats cap al waypoint (amb una petita tolerància)
        if (Quaternion.Angle(transform.rotation, targetRotation) < 2.0f)
        {
            // Quan estem orientats, comencem a moure'ns
            agent.SetDestination(waypoints[targetWaypointIndex].Position);
            currentState = NavState.Moving;
        }
    }

    private void UpdateMovement()
    {
        // Comprovar si hem arribat al waypoint actual
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Hem arribat al waypoint, actualitzem l'índex actual
            currentWaypointIndex = targetWaypointIndex;
            
            // Comencem a rotar cap a l'orientació del waypoint
            currentState = NavState.RotatingAtWaypoint;
        }
    }

    private void UpdateRotationAtWaypoint()
    {
        // Obtenir l'orientació desitjada del waypoint actual
        Quaternion targetRotation = waypoints[currentWaypointIndex].Rotation;
        
        // Rotar gradualment cap a l'orientació desitjada
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
            angularSpeed * Time.deltaTime);
        
        // Comprovar si hem acabat de rotar (amb una petita tolerància)
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        {
            // Comencem a esperar
            waitTimer = waypoints[currentWaypointIndex].waitTime;
            currentState = NavState.Waiting;
        }
    }

    private void UpdateWaiting()
    {
        // Disminuir el temps d'espera
        waitTimer -= Time.deltaTime;
        
        // Comprovar si hem d'avançar al següent waypoint
        if (waitTimer <= 0)
        {
            // Calcular el següent waypoint
            targetWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            
            // Primer girem cap al següent waypoint
            currentState = NavState.RotatingBeforeMoving;
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || waypoints.Count == 0) return;

        // Dibuixar línies entre els waypoints per visualitzar la ruta
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            
            // Dibuixar una línia des d'aquest waypoint al següent
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].Position, waypoints[nextIndex].Position);
            }
        }

        // Si estem en estat de joc, dibuixar l'estat actual
        if (Application.isPlaying)
        {
            // Dibuixar una línia des de la posició actual al waypoint objectiu
            if (targetWaypointIndex < waypoints.Count && waypoints[targetWaypointIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, waypoints[targetWaypointIndex].Position);
            }
        }
    }
}