using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Interface for interactable objects in the world.
/// ES: Interfaz para objetos interactuables del mundo.
/// </summary>
public interface IInteractable
{
    /// <summary>EN: Check if object can be interacted. ES: Verificar si se puede interactuar.</summary>
    bool CanInteract();
    
    /// <summary>EN: Execute interaction logic. ES: Ejecutar lógica de interacción.</summary>
    void OnInteract();
}

/// <summary>
/// EN: Interactive object with state machine and cooldown.
/// ES: Objeto interactivo con máquina de estados y cooldown.
/// </summary>
[RequireComponent(typeof(Collider))]
public class InteractiveObject : MonoBehaviour, IInteractable
{
    private enum InteractionState { Idle, Hovering, Interacting, Cooldown }
    
    [SerializeField]
    [Tooltip("Distancia máxima para interactuar")]
    private float interactionRange = 5f;
    
    [SerializeField]
    [Tooltip("Tiempo de espera entre interacciones")]
    private float cooldownTime = 0.5f;
    
    private InteractionState currentState = InteractionState.Idle;
    private float cooldownTimer = 0f;
    
    [HideInInspector] public UnityEngine.Events.UnityEvent onInteractionStart;
    [HideInInspector] public UnityEngine.Events.UnityEvent onInteractionEnd;
    
    private void Update()
    {
        UpdateCooldown();
    }
    
    /// <summary>
    /// EN: Update cooldown timer and transition from cooldown.
    /// ES: Actualizar temporizador de cooldown y transición desde enfriamiento.
    /// </summary>
    private void UpdateCooldown()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else if (currentState == InteractionState.Cooldown)
        {
            currentState = InteractionState.Idle;
        }
    }
    
    /// <summary>
    /// EN: Check if interaction is possible (not in cooldown, in range, visible).
    /// ES: Verificar si la interacción es posible (no en cooldown, a rango, visible).
    /// </summary>
    public bool CanInteract()
    {
        return currentState != InteractionState.Cooldown && 
               Vector3.Distance(transform.position, GetPlayerPosition()) <= interactionRange &&
               IsVisible();
    }
    
    /// <summary>
    /// EN: Execute interaction with state transition and cooldown.
    /// ES: Ejecutar interacción con transición de estado y cooldown.
    /// </summary>
    public void OnInteract()
    {
        if (!CanInteract()) return;
        
        currentState = InteractionState.Interacting;
        onInteractionStart?.Invoke();
        
        ExecuteInteraction();
        
        onInteractionEnd?.Invoke();
        
        currentState = InteractionState.Cooldown;
        cooldownTimer = cooldownTime;
    }
    
    /// <summary>EN: Execute game logic for this interaction. ES: Ejecutar lógica de juego.</summary>
    private void ExecuteInteraction()
    {
        Debug.Log($"Interacting with {gameObject.name}");
    }
    
    /// <summary>EN: Get player position from cache. ES: Obtener posición del jugador.</summary>
    private Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player ? player.transform.position : Vector3.zero;
    }
    
    /// <summary>EN: Check if object is visible to camera. ES: Verificar si es visible a cámara.</summary>
    private bool IsVisible()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return true;
        return renderer.isVisible;
    }
}

/// <summary>
/// EN: Raycast-based interaction detector. Manages hover and click states.
/// ES: Detector de interacción basado en raycast. Gestiona estados hover y click.
/// </summary>
[RequireComponent(typeof(Camera))]
public class InteractionDetector : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Capa de raycast para interactivos")]
    private LayerMask interactionLayer;
    
    [Tooltip("Mouse position para raycast")]
    private Camera mainCamera;
    
    private IInteractable hoveredObject;
    private List<IInteractable> interactablesInRange = new List<IInteractable>();
    
    [HideInInspector] public UnityEngine.Events.UnityEvent onObjectHovered;
    [HideInInspector] public UnityEngine.Events.UnityEvent onObjectUnhovered;
    
    private void Start()
    {
        mainCamera = GetComponent<Camera>();
    }
    
    private void Update()
    {
        DetectInteractableObjects();
        HandleInteractionInput();
    }
    
    /// <summary>
    /// EN: Raycast from camera to detect interactable objects.
    /// ES: Raycast desde cámara para detectar objetos interactuables.
    /// </summary>
    private void DetectInteractableObjects()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactionLayer))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                hoveredObject = interactable;
                
                if (interactable.CanInteract())
                {
                    onObjectHovered?.Invoke();
                }
            }
        }
        else
        {
            if (hoveredObject != null)
                onObjectUnhovered?.Invoke();
            
            hoveredObject = null;
        }
    }
    
    /// <summary>
    /// EN: Handle mouse input for interactions using LINQ.
    /// ES: Manejar entrada de mouse para interacciones con LINQ.
    /// </summary>
    private void HandleInteractionInput()
    {
        if (Input.GetMouseButtonDown(0) && hoveredObject != null)
        {
            hoveredObject.OnInteract();
        }
    }
    
    /// <summary>
    /// EN: Get all interactables within range (utility with LINQ).
    /// ES: Obtener todos los interactuables a rango (utilidad con LINQ).
    /// </summary>
    public List<IInteractable> GetInteractablesInRange(Vector3 position, float range)
    {
        return interactablesInRange
            .Where(i => Vector3.Distance(((MonoBehaviour)i).transform.position, position) <= range)
            .ToList();
    }
}
