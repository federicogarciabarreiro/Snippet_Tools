using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// EN: Ragdoll controller for physics-based character interactions.
/// ES: Controlador de ragdoll para interacciones basadas en física.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class RagdollController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cuerpos rigidbody del ragdoll")]
    private Rigidbody[] ragdollBodies;
    
    [SerializeField]
    [Tooltip("Umbral de magnitud para activar ragdoll")]
    private float ragdollActivationThreshold = 50f;
    
    [SerializeField]
    [Tooltip("Daño máximo por impacto")]
    private float maxDamagePerImpact = 100f;
    
    private Animator animator;
    private Rigidbody rb;
    private bool isRagdoll = false;
    private float lastImpactMagnitude = 0f;
    
    [HideInInspector] public UnityEngine.Events.UnityEvent<float> onDamageTaken;
    [HideInInspector] public UnityEngine.Events.UnityEvent onRagdollActivated;
    [HideInInspector] public UnityEngine.Events.UnityEvent onRagdollDeactivated;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        DisableRagdoll();
    }
    
    /// <summary>
    /// EN: Handle impact with damage calculation and physics application.
    /// ES: Manejar impacto con cálculo de daño y aplicación de física.
    /// </summary>
    public void OnImpact(Vector3 impactPoint, Vector3 impactForce, float impactMagnitude = -1f)
    {
        if (impactMagnitude < 0)
            impactMagnitude = impactForce.magnitude;
        
        lastImpactMagnitude = impactMagnitude;
        
        // EN: Calculate damage from impact
        // ES: Calcular daño del impacto
        float damage = CalculateDamage(impactMagnitude);
        ApplyDamage(damage);
        
        // EN: Apply physics force to nearest body
        // ES: Aplicar fuerza física al cuerpo más cercano
        ApplyImpactForce(impactPoint, impactForce);
        
        // EN: Check if impact exceeds ragdoll threshold
        // ES: Verificar si el impacto excede el umbral de ragdoll
        if (impactMagnitude > ragdollActivationThreshold && !isRagdoll)
        {
            ActivateRagdoll(impactForce);
        }
    }
    
    /// <summary>
    /// EN: Calculate damage using impact magnitude and mass (F=ma).
    /// ES: Calcular daño usando magnitud de impacto y masa.
    /// </summary>
    private float CalculateDamage(float impactMagnitude)
    {
        // EN: Damage = magnitude * mass * damping factor
        // ES: Daño = magnitud * masa * factor de amortiguamiento
        float baseMass = rb.mass;
        float damage = impactMagnitude * baseMass * 0.1f;
        
        return Mathf.Clamp(damage, 0f, maxDamagePerImpact);
    }
    
    /// <summary>EN: Apply damage to health system. ES: Aplicar daño al sistema de salud.</summary>
    private void ApplyDamage(float damage)
    {
        Debug.Log($"Damage applied: {damage:F2} HP");
        onDamageTaken?.Invoke(damage);
    }
    
    /// <summary>
    /// EN: Apply impact force to nearest ragdoll body using LINQ.
    /// ES: Aplicar fuerza de impacto al cuerpo más cercano con LINQ.
    /// </summary>
    private void ApplyImpactForce(Vector3 point, Vector3 force)
    {
        if (ragdollBodies == null || ragdollBodies.Length == 0)
            return;
        
        // EN: Find nearest ragdoll body using LINQ
        // ES: Encontrar cuerpo ragdoll más cercano con LINQ
        var nearestRagdoll = ragdollBodies
            .AsEnumerable()
            .OrderBy(r => Vector3.Distance(r.position, point))
            .FirstOrDefault();
        
        if (nearestRagdoll != null)
            nearestRagdoll.AddForceAtPosition(force, point, ForceMode.Impulse);
    }
    
    /// <summary>
    /// EN: Transition from animation to ragdoll state.
    /// ES: Transición de animación a estado de ragdoll.
    /// </summary>
    public void ActivateRagdoll(Vector3 impactDirection)
    {
        if (isRagdoll) return;
        
        isRagdoll = true;
        
        // EN: Disable animator to enable physics control
        // ES: Desactivar animator para habilitar control de física
        animator.enabled = false;
        rb.isKinematic = false;
        
        // EN: Enable physics bodies with impact force
        // ES: Habilitar cuerpos de física con fuerza de impacto
        foreach (var body in ragdollBodies)
        {
            body.isKinematic = false;
            body.velocity = impactDirection;
            body.constraints = RigidbodyConstraints.None;
        }
        
        onRagdollActivated?.Invoke();
        Debug.Log("Ragdoll activated");
    }
    
    /// <summary>
    /// EN: Reset to animation-controlled state.
    /// ES: Reiniciar a estado controlado por animación.
    /// </summary>
    public void DisableRagdoll()
    {
        isRagdoll = false;
        animator.enabled = true;
        rb.isKinematic = true;
        
        // EN: Reset all ragdoll bodies to kinematic
        // ES: Reiniciar todos los cuerpos a estado cinemático
        foreach (var body in ragdollBodies)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        onRagdollDeactivated?.Invoke();
    }
    
    /// <summary>EN: Check if currently in ragdoll state. ES: Verificar si está en ragdoll.</summary>
    public bool IsRagdoll => isRagdoll;
    
    /// <summary>EN: Get last impact magnitude. ES: Obtener última magnitud de impacto.</summary>
    public float GetLastImpactMagnitude() => lastImpactMagnitude;
}
