using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Dynamic camera state manager with smooth transitions, priority switching, and cinematic control.
/// ES: Gestor de estado de cámara dinámico con transiciones suaves, cambio de prioridad y control cinemático.
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraStateManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cámara virtual para seguimiento del jugador")]
    private CinemachineVirtualCamera followCamera;

    [SerializeField]
    [Tooltip("Cámara virtual para mirar objetivo")]
    private CinemachineVirtualCamera lookAtCamera;

    [SerializeField]
    [Tooltip("Cámara virtual para secuencias cinemáticas")]
    private CinemachineVirtualCamera cinematicCamera;

    private CinemachineVirtualCamera activeCamera;
    
    [SerializeField]
    [Tooltip("Duración de transición suave entre cámaras")]
    private float transitionDuration = 1f;

    public enum CameraState { Follow, LookAt, Cinematic }
    private CameraState currentState = CameraState.Follow;
    private List<CinemachineVirtualCamera> cameraStack = new List<CinemachineVirtualCamera>();

    private void Start()
    {
        SwitchToCamera(followCamera, CameraState.Follow);
    }

    /// <summary>
    /// EN: Switch to a new camera with state transition and priority update.
    /// ES: Cambia a una nueva cámara con transición de estado y actualización de prioridad.
    /// </summary>
    public void SwitchToCamera(CinemachineVirtualCamera newCamera, CameraState state)
    {
        if (activeCamera != null)
            activeCamera.Priority = 0;  // EN: Deactivate old camera / ES: Desactivar cámara anterior

        activeCamera = newCamera;
        activeCamera.Priority = 10;  // EN: Activate new camera / ES: Activar nueva cámara
        currentState = state;

        // EN: Smooth transition / ES: Transición suave
        StartCoroutine(SmoothCameraTransition());

        Debug.Log($"Camera switched to: {state}");
    }

    /// <summary>
    /// EN: Smooth camera transition using easing curve.
    /// ES: Transición suave de cámara usando curva de suavizado.
    /// </summary>
    private IEnumerator SmoothCameraTransition()
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // EN: Apply easing function / ES: Aplicar función de suavizado
            float eased = Mathf.SmoothStep(0f, 1f, t);

            yield return null;
        }
    }

    /// <summary>
    /// EN: Configure follow camera with target and offset.
    /// ES: Configura cámara de seguimiento con objetivo y desplazamiento.
    /// </summary>
    public void ConfigureFollowCamera(Transform target, Vector3 offset)
    {
        var transposer = followCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = offset;
        }

        followCamera.Follow = target;
        Debug.Log($"Follow camera configured for target: {target.name}");
    }

    /// <summary>
    /// EN: Apply camera shake/noise effect for cinematics.
    /// ES: Aplicar efecto de temblor de cámara para cinemáticas.
    /// </summary>
    public void ApplyNoise(float amplitude, float frequency)
    {
        var noise = activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise != null)
        {
            noise.m_AmplitudeGain = amplitude;
            noise.m_FrequencyGain = frequency;
        }
    }

    /// <summary>
    /// EN: Play cinematic camera shake effect.
    /// ES: Reproduce efecto de temblor de cámara cinemático.
    /// </summary>
    public void PlayCinematicShake(float duration, float intensity)
    {
        StartCoroutine(CameraShakeCoroutine(duration, intensity));
    }

    private IEnumerator CameraShakeCoroutine(float duration, float intensity)
    {
        var noise = activeCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);  // EN: Decay intensity / ES: Disminuir intensidad

            if (noise != null)
                noise.m_AmplitudeGain = intensity * t;

            yield return null;
        }
    }

    public CameraState GetCurrentState() => currentState;
}

/// <summary>
/// EN: Cinematic camera controller for cutscenes with pan and look-ahead.
/// ES: Controlador de cámara cinemática para cinemáticas con barrido y mirada adelantada.
/// </summary>
public class CinematicCamera : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cámara virtual para cinemáticas")]
    private CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    [Tooltip("Curva de amortiguamiento para animación")]
    private AnimationCurve dampingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    [Tooltip("Cantidad de mirada adelantada")]
    private float baseLookAheadAmount = 5f;

    private void Start()
    {
        ConfigureLookAhead();
    }

    /// <summary>
    /// EN: Configure look-ahead offset for target framing.
    /// ES: Configura desplazamiento de mirada adelantada para encuadre de objetivo.
    /// </summary>
    private void ConfigureLookAhead()
    {
        var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
        {
            composer.m_TrackedObjectOffset = Vector3.up * baseLookAheadAmount;
        }
    }

    /// <summary>
    /// EN: Play a camera pan to target position and rotation.
    /// ES: Reproduce un barrido de cámara a posición y rotación objetivo.
    /// </summary>
    public void PlayCutsceneCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        StartCoroutine(PanCamera(targetPosition, targetRotation, duration));
    }

    private IEnumerator PanCamera(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // EN: Apply easing curve / ES: Aplicar curva de suavizado
            float eased = dampingCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPos, targetPos, eased);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, eased);

            yield return null;
        }

        Debug.Log("Cutscene camera pan completed");
    }

    /// <summary>
    /// EN: Set new look-ahead distance dynamically.
    /// ES: Establece nueva distancia de mirada adelantada dinámicamente.
    /// </summary>
    public void SetLookAheadAmount(float amount)
    {
        baseLookAheadAmount = amount;
        ConfigureLookAhead();
    }
}
