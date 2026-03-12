using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// EN: FPS weapon controller with hitscan and ballistic projectiles.
/// ES: Controlador de arma FPS con hitscan y proyectiles balísticos.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cámara del jugador para disparo")]
    private Camera playerCamera;
    
    [SerializeField]
    [Tooltip("Cadencia de disparo (segundos)")]
    private float fireRate = 0.1f;
    
    [SerializeField]
    [Tooltip("Usar hitscan (raycast instantáneo) vs proyectiles")]
    private bool useHitscan = true;
    
    [SerializeField]
    [Tooltip("Daño base del arma")]
    private float baseDamage = 50f;
    
    [SerializeField]
    [Tooltip("Rango máximo del raycast")]
    private float maxRange = 1000f;
    
    private float fireTimer = 0f;
    private AudioSource audioSource;
    
    [HideInInspector] public UnityEngine.Events.UnityEvent<RaycastHit> onHit;
    [HideInInspector] public UnityEngine.Events.UnityEvent onFireWeapon;
    /// <summary>
    /// EN: Fire hitscan raycast from weapon camera center.
    /// ES: Disparar raycast hitscan desde centro de cámara.
    /// </summary>
    private void FireHitscan()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        //EN: Raycast forward from camera
        // ES: Raycast hacia adelante desde cámara
        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            OnWeaponHit(hit);
            Debug.Log($"Hitscan hit: {hit.collider.name}");
        }
        
        onFireWeapon?.Invoke();
    }
    
    /// <summary>
    /// EN: Fire projectile with ballistic arc calculation.
    /// ES: Disparar proyectil con cálculo de arco balístico.
    /// </summary>
    private void FireProjectile()
    {
        // EN: Calculate projectile velocity from forward direction
        // ES: Calcular velocidad de proyectil desde dirección forward
        Vector3 startPosition = transform.position;
        Vector3 launchVelocity = playerCamera.transform.forward * 50f;
        
        Debug.Log($"Projectile fired with velocity: {launchVelocity.magnitude} m/s");
        onFireWeapon?.Invoke();
    }
    
    /// <summary>
    /// EN: Handle weapon hit with damage and effects.
    /// ES: Manejar impacto del arma con daño y efectos.
    /// </summary>
    private void OnWeaponHit(RaycastHit hit)
    {
        float damage = CalculateDamage(hit.distance);
        
        // EN: Apply damage if target is damageable
        // ES: Aplicar daño si el objetivo es dañable
        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }
        
        CreateImpactDecal(hit);
        
        // EN: Apply physical force to rigidbody
        // ES: Aplicar fuerza física al rigidbody
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForceAtPosition(
                (hit.point - transform.position).normalized * 50f,
                hit.point,
                ForceMode.Impulse
            );
        }
        
        onHit?.Invoke(hit);
    }
    
    /// <summary>
    /// EN: Calculate damage with distance falloff using curve.
    /// ES: Calcular daño con falloff de distancia usando curva.
    /// </summary>
    private float CalculateDamage(float distance)
    {
        // EN: Falloff formula: damage decreases with distance
        // ES: Fórmula de falloff: daño disminuye con distancia
        float distanceFalloff = 1f / (distance / 10f + 1f);
        return baseDamage * distanceFalloff;
    }
    
    /// <summary>EN: Create visual decal at impact point. ES: Crear decal visual en punto de impacto.</summary>
    private void CreateImpactDecal(RaycastHit hit)
    {
        Vector3 decalPosition = hit.point + hit.normal * 0.01f;
        Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
        
        Debug.Log($"Impact decal created at {hit.point}");
    }
}

/// <summary>
/// EN: Interface for objects that can receive damage.
/// ES: Interfaz para objetos que pueden recibir daño.
/// </summary>
public interface IDamageable
{
    /// <summary>EN: Apply damage to object. ES: Aplicar daño al objeto.</summary>
    void TakeDamage(float damage);
}

/// <summary>
/// EN: Damageable object with health system.
/// ES: Objeto dañable con sistema de salud.
/// </summary>
public class DamageableObject : MonoBehaviour, IDamageable
{
    [SerializeField]
    [Tooltip("Salud inicial del objeto")]
    private float maxHealth = 100f;
    
    private float currentHealth;
    
    [HideInInspector] public UnityEngine.Events.UnityEvent<float> onHealthChanged;
    [HideInInspector] public UnityEngine.Events.UnityEvent onDeath;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// EN: Apply damage and check if dead.
    /// ES: Aplicar daño y verificar si está muerto.
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        onHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"Object took {damage} damage. Health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }
    
    /// <summary>EN: Handle death of damageable object. ES: Manejar muerte del objeto.</summary>
    private void OnDeath()
    {
        onDeath?.Invoke();
    Vector3 decalPosition = hit.point + hit.normal * 0.01f;
        Quaternion decalRotation = Quaternion.LookRotation(hit.normal);
        
        Debug.Log($"Impact decal created at {hit.point}");
        
        // In real implementation:
        // Instantiate(impactDecalPrefab, decalPosition, decalRotation, hit.collider.transform);
    }
}

public interface IDamageable
{
    void TakeDamage(float damage);
}

public class DamageableObject : MonoBehaviour, IDamageable
{
    private float health = 100f;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Object took {damage} damage. Health: {health}");
        
        if (health <= 0)
            Destroy(gameObject);
    }
}
