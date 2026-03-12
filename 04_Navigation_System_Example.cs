using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// EN: Autonomous NPC navigation controller with NavMesh and path smoothing.
/// ES: Controlador de navegación autónoma de NPCs con NavMesh y suavizado de rutas.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AutonomousNavigationController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Distancia para iniciar desaceleración")]
    private float stoppingDistance = 0.5f;
    
    [SerializeField]
    [Tooltip("Intervalo de recalcular ruta (segundos)")]
    private float pathUpdateInterval = 0.2f;
    
    [SerializeField]
    [Tooltip("Curva de velocidad según inclinación del terreno")]
    private AnimationCurve terrainSpeedCurve = AnimationCurve.Linear(0, 1, 90, 0.5f);
    
    private NavMeshAgent agent;
    private Vector3 targetDestination;
    private float pathUpdateTimer = 0f;
    private bool hasPath = false;
    
    [HideInInspector] public UnityEngine.Events.UnityEvent onArrival;
    [HideInInspector] public UnityEngine.Events.UnityEvent onPathFailed;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    private void Start()
    {
        pathUpdateTimer = pathUpdateInterval;
    }
    
    private void Update()
    {
        // EN: Update path periodically for dynamic obstacles
        // ES: Actualizar ruta periódicamente para obstáculos dinámicos
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0)
        {
            UpdatePathToDestination();
            pathUpdateTimer = pathUpdateInterval;
        }
        
        HandleMovement();
    }
    
    /// <summary>
    /// EN: Set destination for NPC to navigate to.
    /// ES: Establecer destino para que el NPC navegue.
    /// </summary>
    public void SetDestination(Vector3 destination)
    {
        targetDestination = destination;
        hasPath = false;
    }
    
    /// <summary>
    /// EN: Update NavMesh path calculation.
    /// ES: Actualizar cálculo de ruta NavMesh.
    /// </summary>
    private void UpdatePathToDestination()
    {
        if (!hasPath)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, targetDestination, 
                NavMesh.AllAreas, path))
            {
                agent.SetPath(path);
                hasPath = true;
                SmoothPath(path);
            }
            else
            {
                onPathFailed?.Invoke();
                hasPath = false;
            }
        }
    }
    
    /// <summary>
    /// EN: Smooth waypoint path for natural movement using Catmull-Rom.
    /// ES: Suavizar ruta de waypoints para movimiento natural.
    /// </summary>
    private void SmoothPath(NavMeshPath path)
    {
        // EN: Interpolate waypoints for smoother curves
        // ES: Interpolar waypoints para curvas más suaves
        for (int i = 1; i < path.corners.Length - 1; i++)
        {
            Vector3 prev = path.corners[i - 1];
            Vector3 curr = path.corners[i];
            Vector3 next = path.corners[i + 1];
            
            path.corners[i] = Vector3.Lerp(curr, (prev + next) / 2f, 0.5f);
        }
    }
    
    /// <summary>
    /// EN: Handle movement and arrival detection with dynamic speed.
    /// ES: Manejar movimiento y detección de llegada con velocidad dinámica.
    /// </summary>
    private void HandleMovement()
    {
        if (!agent.hasPath)
            return;
        
        // EN: Dynamically adjust speed based on terrain slope
        // ES: Ajustar dinámicamente la velocidad según inclinación
        float speedMultiplier = GetTerrainSpeedModifier();
        agent.speed = 5f * speedMultiplier;
        
        // EN: Check if arrived at destination
        // ES: Verificar si llegó al destino
        if (!agent.hasPath || agent.remainingDistance <= stoppingDistance)
        {
            if (!agent.hasPath || !agent.pathPending)
            {
                OnDestinationReached();
            }
        }
    }
    
    /// <summary>
    /// EN: Get speed modifier based on terrain (slope, material, etc).
    /// ES: Obtener modificador de velocidad según terreno.
    /// </summary>
    private float GetTerrainSpeedModifier()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
        {
            // EN: Calculate slope angle from ground normal
            // ES: Calcular ángulo de pendiente desde normal del terreno
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            return terrainSpeedCurve.Evaluate(slope / 90f);
        }
        return 1f;
    }
    
    /// <summary>EN: Callback when destination reached. ES: Callback al llegar al destino.</summary>
    private void OnDestinationReached()
    {
        Debug.Log("Destination reached");
        onArrival?.Invoke();
        hasPath = false;
    }
    
    /// <summary>EN: Get current path remaining distance. ES: Obtener distancia restante.</summary>
    public float GetRemainingDistance() => agent.remainingDistance;
    
    /// <summary>EN: Stop agent immediately. ES: Detener agente inmediatamente.</summary>
    public void Stop()
    {
        agent.ResetPath();
        hasPath = false;
    }
}
