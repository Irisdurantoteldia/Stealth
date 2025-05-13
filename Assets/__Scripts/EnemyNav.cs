using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Assegura't que el component NavMeshAgent està present en el GameObject
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNav : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Llista de punts de pas per patrullar")]
    public List<Waypoint> waypoints = new List<Waypoint>();  // Llista de waypoints que l'enemic ha de seguir
    
    [Tooltip("Velocitat de moviment")]
    public float speed = 4.0f; // Velocitat de moviment de l'enemic
    
    [Tooltip("Velocitat de rotació")]
    public float angularSpeed = 90.0f; // Velocitat de rotació de l'enemic

    [Header("State")]
    [SerializeField] private int currentWaypointIndex = 0; // Índex del waypoint actual
    [SerializeField] private int targetWaypointIndex = 0;  // Índex del waypoint objectiu

    // Components
    private NavMeshAgent agent; // Agent de NavMesh que permet moure l'enemic pel mapa

    // Estat intern de l'enemic durant el patrullatge
    private float waitTimer = 0f; // Temporitzador per esperar als waypoints
    private enum NavState { RotatingBeforeMoving, Moving, RotatingAtWaypoint, Waiting } // Estats en què pot estar l'enemic
    private NavState currentState = NavState.RotatingBeforeMoving; // Estat actual de l'enemic

    [Header("Debug")]
    [SerializeField]
    private bool drawGizmos = true; // Permet dibuixar informació de depuració a l'editor
    [SerializeField]
    private string currentStateDebug = ""; // Mostrar l'estat actual de l'enemic per a depuració

    private void Awake()
    {
        // Obtenim el component NavMeshAgent associat a l'enemic
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Configurem la velocitat i la velocitat de rotació del NavMeshAgent
        agent.speed = speed;
        agent.angularSpeed = angularSpeed;
        agent.updateRotation = false; // Desactivem la rotació automàtica per controlar-la manualment
        
        // Si tenim waypoints definits, iniciem el patrullatge
        if (waypoints.Count > 0)
        {
            currentWaypointIndex = 0;  // Inicialitzem l'índex del primer waypoint
            targetWaypointIndex = 0;   // Inicialitzem l'índex del waypoint objectiu
            currentState = NavState.RotatingBeforeMoving; // Inicialitzem l'estat a "Rotant abans de moure"
        }
    }

    private void Update()
    {
        // Si no tenim waypoints, no fem res
        if (waypoints.Count == 0) return;

        // Actualitzem l'estat de depuració mostrant l'estat actual de l'enemic
        currentStateDebug = currentState.ToString();

        // Depenent de l'estat, cridem la funció corresponent
        switch (currentState)
        {
            case NavState.RotatingBeforeMoving:
                UpdateRotationBeforeMoving(); // Actualitza la rotació abans de començar a moure's
                break;
            case NavState.Moving:
                UpdateMovement(); // Actualitza el moviment quan l'enemic es mou
                break;
            case NavState.RotatingAtWaypoint:
                UpdateRotationAtWaypoint(); // Actualitza la rotació quan l'enemic arribi a un waypoint
                break;
            case NavState.Waiting:
                UpdateWaiting(); // Actualitza el temporitzador d'espera entre waypoints
                break;
        }
    }

    // Aquesta funció controla la rotació de l'enemic abans de començar a moure's
    private void UpdateRotationBeforeMoving()
    {
        // Calculem la direcció cap al següent waypoint
        Vector3 targetDirection = waypoints[targetWaypointIndex].Position - transform.position;
        targetDirection.y = 0; // Assegurem que la rotació es faci només sobre l'eix Y (horitzontal)

        // Si estem massa a prop del waypoint, passem a rotar cap a ell
        if (targetDirection.magnitude < 0.1f)
        {
            currentState = NavState.RotatingAtWaypoint; // Canviem l'estat a rotar al waypoint
            return;
        }

        // Calculem la rotació desitjada per mirar cap al waypoint
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Rotem gradualment cap a la rotació desitjada amb una velocitat angular determinada
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
            angularSpeed * Time.deltaTime);
        
        // Comprovem si ja estem mirant cap al waypoint
        if (Quaternion.Angle(transform.rotation, targetRotation) < 2.0f)
        {
            // Quan estem orientats, comencem a moure'ns
            agent.SetDestination(waypoints[targetWaypointIndex].Position); // Establim el destí del NavMeshAgent
            currentState = NavState.Moving; // Canviem a l'estat de moviment
        }
    }

    // Aquesta funció s'encarrega de moure l'enemic entre els waypoints
    private void UpdateMovement()
    {
        // Comprovem si hem arribat al waypoint objectiu
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = targetWaypointIndex; // Actualitzem l'índex del waypoint actual
            currentState = NavState.RotatingAtWaypoint; // Canviem a l'estat de rotació a l'arribar
        }
    }

    // Aquesta funció actualitza la rotació de l'enemic quan ha arribat al waypoint
    private void UpdateRotationAtWaypoint()
    {
        // Obtenim la rotació desitjada per al waypoint actual
        Quaternion targetRotation = waypoints[currentWaypointIndex].Rotation;
        
        // Rotem gradualment cap a la rotació desitjada
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
            angularSpeed * Time.deltaTime);
        
        // Comprovem si hem acabat de rotar (amb una tolerància)
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        {
            // Quan hem acabat de rotar, comencem a esperar abans de moure'ns al següent waypoint
            waitTimer = waypoints[currentWaypointIndex].waitTime; 
            currentState = NavState.Waiting; // Canviem l'estat a "Esperant"
        }
    }

    // Aquesta funció gestiona el temps d'espera entre waypoints
    private void UpdateWaiting()
    {
        // Disminuïm el temporitzador d'espera
        waitTimer -= Time.deltaTime;
        
        if (waitTimer <= 0)
        {
            // Calcular el següent waypoint (amb cicle, quan arriba al final, torna al primer)
            targetWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            
            // Primer girem cap al següent waypoint
            currentState = NavState.RotatingBeforeMoving;
        }
    }
}
