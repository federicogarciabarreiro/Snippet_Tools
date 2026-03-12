using UnityEngine;
using System.Collections;

/// <summary>
/// EN: State-driven animation controller with procedural IK and audio sync.
/// ES: Controlador de animación controlado por estados con IK procedural y sincronización de audio.
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimationStateController : MonoBehaviour
{
    private enum AnimState { Idle, Walk, Run, Jump, Attack, Hit, Dead }
    
    [SerializeField]
    [Tooltip("Transformada de cabeza para IK look-at")]
    private Animator animator;
    
    [SerializeField]
    [Tooltip("Transformada de cabeza para IK look-at")]
    private Transform headTransform;
    
    [SerializeField]
    [Tooltip("Objetivo de mirada (null = ninguno)")]
    private Transform targetToLookAt;
    
    [SerializeField]
    [Tooltip("Peso IK de mirada (0-1)")]
    private float ikWeight = 0.8f;
    
    [SerializeField]
    [Tooltip("Transiciones suave entre estados")]
    private float stateTransitionSpeed = 0.1f;
    
    private AnimState currentState = AnimState.Idle;
    private AnimState nextState;
    private float inputVertical = 0f;
    
    [HideInInspector] public bool onJump = false;
    [HideInInspector] public bool onAttack = false;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        HandleInput();
        UpdateAnimatorState();
    }
    
    /// <summary>
    /// EN: Read input and determine animation state transition.
    /// ES: Leer entrada y determinar transición de estado de animación.
    /// </summary>
    private void HandleInput()
    {
        inputVertical = Input.GetAxis("Vertical");
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TransitionToState(AnimState.Jump);
            onJump = true;
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TransitionToState(AnimState.Attack);
            onAttack = true;
        }
        else if (inputVertical > 0.5f)
        {
            TransitionToState(AnimState.Run);
        }
        else if (inputVertical > 0.1f)
        {
            TransitionToState(AnimState.Walk);
        }
        else
        {
            TransitionToState(AnimState.Idle);
        }
    }
    
    /// <summary>
    /// EN: Transition to new animation state with parameter updates.
    /// ES: Transición a nuevo estado de animación actualizando parámetros.
    /// </summary>
    private void TransitionToState(AnimState newState)
    {
        if (currentState == newState)
            return;
        
        nextState = newState;
        
        // EN: Update animator state flags
        // ES: Actualizar flags de estado del animator
        animator.SetBool("isMoving", newState != AnimState.Idle && newState != AnimState.Dead);
        
        switch (newState)
        {
            case AnimState.Idle:
                animator.SetTrigger("Idle");
                animator.SetFloat("Speed", 0f, stateTransitionSpeed, Time.deltaTime);
                break;
                
            case AnimState.Walk:
                animator.SetFloat("Speed", 1f, stateTransitionSpeed, Time.deltaTime);
                animator.SetBool("isMoving", true);
                break;
                
            case AnimState.Run:
                animator.SetFloat("Speed", 2f, stateTransitionSpeed, Time.deltaTime);
                animator.SetBool("isMoving", true);
                break;
                
            case AnimState.Jump:
                animator.SetTrigger("Jump");
                StartCoroutine(PlayAudioAtTiming("jump_sound", 0.1f));
                break;
                
            case AnimState.Attack:
                animator.SetTrigger("Attack");
                StartCoroutine(PlayAudioAtTiming("attack_whoosh", 0.15f));
                break;
                
            case AnimState.Hit:
                animator.SetTrigger("Hit");
                break;
                
            case AnimState.Dead:
                animator.SetTrigger("Dead");
                enabled = false;
                break;
        }
        
        currentState = newState;
    }
    
    /// <summary>
    /// EN: Update procedural animations (IK for look-at direction).
    /// ES: Actualizar animaciones procedurales (IK para dirección de mirada).
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        if (targetToLookAt == null || headTransform == null)
            return;
        
        // EN: Apply procedural IK for head look-at
        // ES: Aplicar IK procedural para mirada de cabeza
        animator.SetLookAtWeight(ikWeight);
        animator.SetLookAtPosition(targetToLookAt.position);
    }
    
    /// <summary>
    /// EN: Play audio effect synchronized with animation timeline.
    /// ES: Reproducir efecto de audio sincronizado con línea de tiempo.
    /// </summary>
    private IEnumerator PlayAudioAtTiming(string audioKey, float delaySeconds)
    {
        // EN: Use coroutine to delay audio playback to animation frame
        // ES: Usar corrutina para retrasar reproducción de audio
        yield return new WaitForSeconds(delaySeconds);
        PlaySoundEffect(audioKey);
    }
    
    /// <summary>EN: Execute sound effect. ES: Ejecutar efecto de sonido.</summary>
    private void PlaySoundEffect(string audioKey)
    {
        Debug.Log($"Sound effect triggered: {audioKey}");
    }
    
    /// <summary>EN: Get current animation state. ES: Obtener estado de animación actual.</summary>
    public AnimState GetCurrentState() => currentState;
    
    /// <summary>EN: Check if animation is playing specific clip. ES: Verificar si está reproduciendo clip específico.</summary>
    public bool IsPlayingAnimation(string animationName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
    
    private void UpdateAnimatorState()
    {
        // EN: Update animator based on current state
        // ES: Actualizar animator según el estado actual
    }
}
