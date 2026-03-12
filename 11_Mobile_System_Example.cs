using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Mobile touch input manager with gesture detection and haptic feedback.
/// ES: Gestor de entrada táctil con detección de gestos y retroalimentación háptica.
/// </summary>
public class TouchInputManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Distancia mínima en píxeles para detectar gesto de deslizamiento")]
    private float swipeThreshold = 100f;

    [SerializeField]
    [Tooltip("Tiempo máximo entre dos toques para detectar doble toque")]
    private float doubleTapTimeThreshold = 0.3f;

    [SerializeField]
    [Tooltip("Tiempo de pulsación larga en segundos")]
    private float longPressThreshold = 0.5f;

    private Vector2 touchStartPosition;
    private float lastTapTime = 0f;
    private float touchStartTime = 0f;
    private Touch[] touches;
    private bool onLongPress = false;

    // EN: Gesture event delegates / ES: Delegados de eventos de gestos
    public delegate void TouchGestureCallback(Vector2 position);
    public event TouchGestureCallback OnTap;
    public event TouchGestureCallback OnDoubleTap;
    public event TouchGestureCallback OnLongPress;
    
    public delegate void SwipeCallback(Vector2 direction);
    public event SwipeCallback OnSwipe;

    private void Update()
    {
        DetectTouches();
    }

    /// <summary>
    /// EN: Detect and process all touch inputs.
    /// ES: Detectar y procesar todas las entradas táctiles.
    /// </summary>
    private void DetectTouches()
    {
        touches = Input.touches;

        // EN: Use LINQ to process touches / ES: Usar LINQ para procesar toques
        touches.ForEach(touch => ProcessTouchPhase(touch));
    }

    /// <summary>
    /// EN: Process touch based on its phase.
    /// ES: Procesar toque según su fase.
    /// </summary>
    private void ProcessTouchPhase(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchBegan(touch);
                break;
            case TouchPhase.Moved:
                HandleTouchMoved(touch);
                break;
            case TouchPhase.Ended:
                HandleTouchEnded(touch);
                break;
            case TouchPhase.Canceled:
                HandleTouchCanceled(touch);
                break;
        }
    }

    /// <summary>
    /// EN: Handle touch began event for double-tap detection.
    /// ES: Manejar evento de toque iniciado para detección de doble toque.
    /// </summary>
    private void HandleTouchBegan(Touch touch)
    {
        touchStartPosition = touch.position;
        touchStartTime = Time.time;
        onLongPress = false;

        // EN: Check for double tap / ES: Verificar doble toque
        if (Time.time - lastTapTime < doubleTapTimeThreshold)
        {
            OnDoubleTap?.Invoke(touch.position);
            HapticFeedbackManager.PlaySuccessFeedback();
        }

        lastTapTime = Time.time;
    }

    /// <summary>
    /// EN: Handle touch moved event for swipe detection.
    /// ES: Manejar evento de toque movido para detección de deslizamiento.
    /// </summary>
    private void HandleTouchMoved(Touch touch)
    {
        // EN: Detect swipe / ES: Detectar deslizamiento
        Vector2 swipeDelta = touch.position - touchStartPosition;
        
        if (swipeDelta.magnitude > swipeThreshold)
        {
            Vector2 swipeDirection = swipeDelta.normalized;
            OnSwipe?.Invoke(swipeDirection);
            HapticFeedbackManager.PlayTapFeedback();
        }

        // EN: Check for long press / ES: Verificar pulsación larga
        if (!onLongPress && Time.time - touchStartTime > longPressThreshold)
        {
            onLongPress = true;
            OnLongPress?.Invoke(touch.position);
            HapticFeedbackManager.PlayErrorFeedback();
        }
    }

    /// <summary>
    /// EN: Handle touch ended event.
    /// ES: Manejar evento de toque finalizado.
    /// </summary>
    private void HandleTouchEnded(Touch touch)
    {
        Vector2 swipeDelta = touch.position - touchStartPosition;

        // EN: If small movement, it's a tap / ES: Si movimiento pequeño, es un toque
        if (swipeDelta.magnitude < swipeThreshold && !onLongPress)
        {
            OnTap?.Invoke(touch.position);
            HapticFeedbackManager.PlayTapFeedback();
        }
    }

    private void HandleTouchCanceled(Touch touch)
    {
        // EN: Touch was canceled / ES: Toque fue cancelado
        onLongPress = false;
    }
}

/// <summary>
/// EN: Multi-touch gesture detector for pinch zoom and rotation.
/// ES: Detector de gestos multi-toque para zoom de pellizco y rotación.
/// </summary>
public class MultiTouchGestureDetector : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Umbral mínimo de cambio de distancia para detectar pellizco")]
    private float pinchThreshold = 0.1f;

    public delegate void PinchCallback(float scale);
    public event PinchCallback OnPinch;

    public delegate void RotationCallback(float angle);
    public event RotationCallback OnRotation;

    private float previousPinchDistance = 0f;
    private float previousRotationAngle = 0f;

    private void Update()
    {
        if (Input.touchCount == 2)
        {
            // EN: Detect multi-touch gestures / ES: Detectar gestos multi-toque
            DetectPinchGesture();
            DetectRotationGesture();
        }
    }

    /// <summary>
    /// EN: Detect pinch zoom gesture.
    /// ES: Detectar gesto de zoom de pellizco.
    /// </summary>
    private void DetectPinchGesture()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        if (previousPinchDistance > 0)
        {
            float pinchDelta = currentDistance - previousPinchDistance;

            if (Mathf.Abs(pinchDelta) > pinchThreshold)
            {
                float scale = 1f + (pinchDelta / 100f);
                OnPinch?.Invoke(scale);
            }
        }

        previousPinchDistance = currentDistance;
    }

    /// <summary>
    /// EN: Detect two-finger rotation gesture.
    /// ES: Detectar gesto de rotación de dos dedos.
    /// </summary>
    private void DetectRotationGesture()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 direction = (touch1.position - touch0.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (previousRotationAngle != 0)
        {
            float rotationDelta = angle - previousRotationAngle;
            OnRotation?.Invoke(rotationDelta);
        }

        previousRotationAngle = angle;
    }
}

/// <summary>
/// EN: Haptic feedback manager for platform-specific vibrations.
/// ES: Gestor de retroalimentación háptica para vibraciones específicas de plataforma.
/// </summary>
public class HapticFeedbackManager : MonoBehaviour
{
    /// <summary>
    /// EN: Play simple tap vibration feedback.
    /// ES: Reproducir retroalimentación de vibración de toque simple.
    /// </summary>
    public static void PlayTapFeedback()
    {
        #if UNITY_IOS
            Handheld.Vibrate();
        #elif UNITY_ANDROID
            Vibration.Vibrate(50);  // EN: 50ms vibration / ES: Vibración de 50ms
        #endif
    }

    /// <summary>
    /// EN: Play success vibration pattern.
    /// ES: Reproducir patrón de vibración de éxito.
    /// </summary>
    public static void PlaySuccessFeedback()
    {
        #if UNITY_ANDROID
            // EN: Pattern: wait 0, vibrate 100, wait 150, vibrate 100 / ES: Patrón específico
            Vibration.Vibrate(new long[] { 0, 100, 150, 100 });
        #elif UNITY_IOS
            Handheld.Vibrate();
        #endif
    }

    /// <summary>
    /// EN: Play error vibration pattern.
    /// ES: Reproducir patrón de vibración de error.
    /// </summary>
    public static void PlayErrorFeedback()
    {
        #if UNITY_ANDROID
            // EN: Long buzz pattern / ES: Patrón de zumbido largo
            Vibration.Vibrate(new long[] { 0, 200 });
        #elif UNITY_IOS
            Handheld.Vibrate();
        #endif
    }
}

/// <summary>
/// EN: Hardware permission manager for mobile platforms.
/// ES: Gestor de permisos de hardware para plataformas móviles.
/// </summary>
public class HardwarePermissionManager : MonoBehaviour
{
    /// <summary>
    /// EN: Request camera permission from user.
    /// ES: Solicitar permiso de cámara del usuario.
    /// </summary>
    public static bool RequestCameraPermission()
    {
        #if UNITY_ANDROID
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.Camera);
        #elif UNITY_IOS
            return Application.HasUserAuthorization(UserAuthorization.WebCam);
        #else
            return true;
        #endif
    }

    /// <summary>
    /// EN: Request location permission from user.
    /// ES: Solicitar permiso de ubicación del usuario.
    /// </summary>
    public static bool RequestLocationPermission()
    {
        #if UNITY_ANDROID
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.FineLocation);
        #elif UNITY_IOS
            return true;  // EN: Handled via Info.plist / ES: Manejado vía Info.plist
        #else
            return true;
        #endif
    }

    /// <summary>
    /// EN: Request microphone permission from user.
    /// ES: Solicitar permiso de micrófono del usuario.
    /// </summary>
    public static bool RequestMicrophonePermission()
    {
        #if UNITY_ANDROID
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(
                UnityEngine.Android.Permission.RecordAudio);
        #elif UNITY_IOS
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
        #else
            return true;
        #endif
    }
}

/// <summary>
/// EN: Vibration helper for Android platform-specific vibration patterns.
/// ES: Ayudante de vibración para patrones de vibración específicos de Android.
/// </summary>
public class Vibration
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass unityPlayer;
    private static AndroidJavaObject context;
    private static AndroidJavaClass vibrator;
    #endif

    /// <summary>
    /// EN: Play vibration for specified duration in milliseconds.
    /// ES: Reproducir vibración durante duración especificada en milisegundos.
    /// </summary>
    public static void Vibrate(long milliseconds)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            GetVibrator().Call("vibrate", milliseconds);
        #endif
    }

    /// <summary>
    /// EN: Play vibration pattern.
    /// ES: Reproducir patrón de vibración.
    /// </summary>
    public static void Vibrate(long[] pattern)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            GetVibrator().Call("vibrate", pattern);
        #endif
    }

    #if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject GetVibrator()
    {
        if (vibrator == null)
        {
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }
        return vibrator;
    }
    #endif
}
