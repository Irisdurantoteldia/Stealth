using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Configuració de Patrulla")]
    [Tooltip("Llista de waypoints per on el robot patrullarà")]
    public List<Waypoint> patrolWaypoints = new List<Waypoint>();
    
    [Tooltip("Velocitat de rotació del robot en graus per segon")]
    public float rotationSpeed = 90f;
    
    [Header("Visualització")]
    [Tooltip("Activar per mostrar el recorregut en l'editor")]
    public bool showPath = true;
    
    // Referències als components
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    
    // Variables de control
    private int currentWaypointIndex = 0;
    private bool isRotatingToWaypoint = false;
    private bool isWaitingAtWaypoint = false;
    private bool isRotatingToWaypointDirection = false;
    
    // Estat de l'enemic
    private enum EnemyState
    {
        RotatingToWaypoint,
        MovingToWaypoint,
        RotatingAtWaypoint,
        WaitingAtWaypoint
    }
    
    private EnemyState currentState;
    
    void Start()
    {
        // Obtenir referències als components necessaris
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent no trobat en el robot enemic!");
            enabled = false;
            return;
        }
        
        // Desactivar l'autoorienting per gestionar manualment la rotació
        navMeshAgent.updateRotation = false;
        
        // Assegurar-se que tenim waypoints
        if (patrolWaypoints.Count == 0)
        {
            Debug.LogWarning("No hi ha waypoints assignats al robot enemic!");
            enabled = false;
            return;
        }
        
        // Numerem els waypoints per visualització
        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            if (patrolWaypoints[i] != null)
            {
                patrolWaypoints[i].SetWaypointNumber(i + 1);
            }
        }
        
        // Iniciar la patrulla
        currentState = EnemyState.RotatingToWaypoint;
        StartCoroutine(PatrolRoutine());
    }
    
    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            switch (currentState)
            {
                case EnemyState.RotatingToWaypoint:
                    yield return StartCoroutine(RotateToWaypoint());
                    currentState = EnemyState.MovingToWaypoint;
                    break;
                    
                case EnemyState.MovingToWaypoint:
                    yield return StartCoroutine(MoveToWaypoint());
                    currentState = EnemyState.RotatingAtWaypoint;
                    break;
                    
                case EnemyState.RotatingAtWaypoint:
                    yield return StartCoroutine(RotateToWaypointDirection());
                    currentState = EnemyState.WaitingAtWaypoint;
                    break;
                    
                case EnemyState.WaitingAtWaypoint:
                    yield return StartCoroutine(WaitAtWaypoint());
                    // Passar al següent waypoint
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
                    currentState = EnemyState.RotatingToWaypoint;
                    break;
            }
            
            yield return null;
        }
    }
    
    IEnumerator RotateToWaypoint()
    {
        if (patrolWaypoints[currentWaypointIndex] == null) yield break;
        
        Vector3 targetDirection = patrolWaypoints[currentWaypointIndex].GetPosition() - transform.position;
        targetDirection.y = 0; // Mantenir la rotació només en el pla XZ
        
        if (targetDirection.magnitude < 0.1f)
        {
            // Si estem molt a prop, anem directament al següent estat
            yield break;
        }
        
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        
        // Activar animació de rotació si existeix
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2.0f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
    
    IEnumerator MoveToWaypoint()
    {
        if (patrolWaypoints[currentWaypointIndex] == null) yield break;
        
        // Configurar el navMeshAgent per moure's cap al waypoint
        navMeshAgent.SetDestination(patrolWaypoints[currentWaypointIndex].GetPosition());
        
        // Activar l'animació de caminar si existeix
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        // Esperar fins que arribem al destí o ens hi apropem prou
        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }
        
        // Desactivar l'animació de caminar
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
    }
    
    IEnumerator RotateToWaypointDirection()
    {
        if (patrolWaypoints[currentWaypointIndex] == null) yield break;
        
        Quaternion targetRotation = patrolWaypoints[currentWaypointIndex].GetRotation();
        
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2.0f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
    
    IEnumerator WaitAtWaypoint()
    {
        if (patrolWaypoints[currentWaypointIndex] == null) yield break;
        
        // Esperar el temps indicat en el waypoint
        float waitTime = patrolWaypoints[currentWaypointIndex].waitTime;
        yield return new WaitForSeconds(waitTime);
    }
    
    void OnDrawGizmos()
    {
        if (!showPath || patrolWaypoints.Count <= 1) return;
        
        // Dibuixar línies connectant els waypoints
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            if (patrolWaypoints[i] == null) continue;
            
            Vector3 currentWaypointPos = patrolWaypoints[i].transform.position;
            Vector3 nextWaypointPos;
            
            // Connectar amb el següent waypoint o amb el primer si és l'últim
            if (i < patrolWaypoints.Count - 1 && patrolWaypoints[i + 1] != null)
            {
                nextWaypointPos = patrolWaypoints[i + 1].transform.position;
            }
            else if (patrolWaypoints[0] != null)
            {
                nextWaypointPos = patrolWaypoints[0].transform.position;
            }
            else
            {
                continue;
            }
            
            // Dibuixar la línia
            Gizmos.DrawLine(currentWaypointPos, nextWaypointPos);
            
            // Dibuixar una fletxa a la meitat del camí
            Vector3 direction = nextWaypointPos - currentWaypointPos;
            Vector3 midPoint = currentWaypointPos + direction * 0.5f;
            float arrowSize = 0.3f;
            
            // Dibuixar una petita fletxa per indicar la direcció
            Vector3 normalizedDir = direction.normalized;
            Vector3 right = Quaternion.Euler(0, -30, 0) * normalizedDir * arrowSize;
            Vector3 left = Quaternion.Euler(0, 30, 0) * normalizedDir * arrowSize;
            
            Gizmos.DrawRay(midPoint, -right);
            Gizmos.DrawRay(midPoint, -left);
        }
    }
}