using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Streaming video player with adaptive caching, subtitle synchronization, and playback quality control.
/// ES: Reproductor de video en streaming con almacenamiento adaptativo, sincronización de subtítulos y control de calidad de reproducción.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class StreamingVideoPlayer : MonoBehaviour
{
    [System.Serializable]
    public class SubtitleTrack
    {
        public string language;
        public List<SubtitleLine> lines = new List<SubtitleLine>();
    }

    [System.Serializable]
    public class SubtitleLine
    {
        public float startTime;
        public float endTime;
        public string text;
    }

    [SerializeField]
    [Tooltip("URL de video para streaming (local o remoto)")]
    private string videoURL;

    [SerializeField]
    [Tooltip("Usar caché local para mejores tiempos de inicio")]
    private bool useLocalCache = true;

    [SerializeField]
    [Tooltip("Directorio para almacenar copias en caché de videos")]
    private string cacheDirectory = "VideoCache";

    [SerializeField]
    [Tooltip("Pistas de subtítulos disponibles")]
    private List<SubtitleTrack> subtitleTracks = new List<SubtitleTrack>();

    private VideoPlayer videoPlayer;
    private bool isPlaying = false;
    private bool onLoop = false;
    private float playbackSpeed = 1f;
    private float currentPlayTime = 0f;

    private void OnEnable()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        InitializeVideoPlayer();
    }

    /// <summary>
    /// EN: Initialize video player with event subscriptions and streaming configuration.
    /// ES: Inicializa el reproductor de video con suscripciones de eventos y configuración de streaming.
    /// </summary>
    private void InitializeVideoPlayer()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;
    }

    /// <summary>
    /// EN: Play a video from URL with optional caching.
    /// ES: Reproduce un video desde URL con almacenamiento en caché opcional.
    /// </summary>
    public void PlayVideo(string url)
    {
        // EN: Check cache first / ES: Verificar caché primero
        if (useLocalCache && IsCached(url))
        {
            videoPlayer.url = GetCachedPath(url);
            Debug.Log($"Playing from cache: {url}");
        }
        else
        {
            videoPlayer.url = url;
        }

        videoPlayer.Play();
        isPlaying = true;

        // EN: Start caching in background / ES: Comenzar almacenamiento en segundo plano
        if (useLocalCache)
            StartCoroutine(CacheVideoProgressive(url));
    }

    /// <summary>
    /// EN: Pause the currently playing video.
    /// ES: Pausa el video que se reproduce actualmente.
    /// </summary>
    public void PauseVideo()
    {
        videoPlayer.Pause();
        isPlaying = false;
    }

    /// <summary>
    /// EN: Resume playing the paused video.
    /// ES: Reanuda la reproducción del video pausado.
    /// </summary>
    public void ResumeVideo()
    {
        videoPlayer.Play();
        isPlaying = true;
    }

    /// <summary>
    /// EN: Seek to specific time in video.
    /// ES: Buscar tiempo específico en el video.
    /// </summary>
    public void SeekToTime(double seconds)
    {
        videoPlayer.time = seconds;
    }

    /// <summary>
    /// EN: Set playback speed.
    /// ES: Establecer velocidad de reproducción.
    /// </summary>
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = Mathf.Clamp(speed, 0.5f, 2f);
        videoPlayer.playbackSpeed = playbackSpeed;
    }

    private bool IsCached(string url)
    {
        string cachedPath = GetCachedPath(url);
        return System.IO.File.Exists(cachedPath);
    }

    private string GetCachedPath(string url)
    {
        string cacheFile = System.IO.Path.Combine(
            Application.persistentDataPath,
            cacheDirectory,
            url.GetHashCode().ToString() + ".mp4"
        );
        return cacheFile;
    }

    /// <summary>
    /// EN: Cache video file progressively during playback.
    /// ES: Almacenar video en caché progressivamente durante la reproducción.
    /// </summary>
    private IEnumerator CacheVideoProgressive(string url)
    {
        string cachedPath = GetCachedPath(url);
        string cacheDir = System.IO.Path.GetDirectoryName(cachedPath);

        if (!System.IO.Directory.Exists(cacheDir))
            System.IO.Directory.CreateDirectory(cacheDir);

        // EN: Download and cache video progressively / ES: Descargar y almacenar en caché progresivamente
        using (var www = new WWW(url))
        {
            while (!www.isDone)
            {
                Debug.Log($"Download progress: {www.progress:P0}");
                yield return null;
            }

            if (string.IsNullOrEmpty(www.error))
            {
                System.IO.File.WriteAllBytes(cachedPath, www.bytes);
                Debug.Log($"Video cached: {cachedPath}");
            }
            else
            {
                Debug.LogError($"Cache download error: {www.error}");
            }
        }
    }

    public float GetCurrentTime() => (float)videoPlayer.time;
    public float GetDuration() => (float)videoPlayer.length;
    public float GetProgress() => GetDuration() > 0 ? GetCurrentTime() / GetDuration() : 0f;
    public bool IsPlaying() => isPlaying;

    private void OnVideoStarted(VideoPlayer vp)
    {
        Debug.Log("Video started");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished");
        isPlaying = false;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"Video error: {message}");
        isPlaying = false;
    }

    public void SetLoop(bool loop)
    {
        onLoop = loop;
        videoPlayer.loopPointReached -= OnVideoFinished;
        
        if (loop)
            videoPlayer.isLooping = true;
    }
}

/// <summary>
/// EN: Synchronizes video playback with subtitle display.
/// ES: Sincroniza la reproducción de video con la visualización de subtítulos.
/// </summary>
public class SubtitleSynchronizer : MonoBehaviour
{
    [System.Serializable]
    public class Subtitle
    {
        public double startTime;
        public double endTime;
        [TextArea(2, 4)]
        public string text;
    }

    [SerializeField]
    [Tooltip("Reproductor de video a sincronizar")]
    private StreamingVideoPlayer videoPlayer;

    [SerializeField]
    [Tooltip("Elemento UI para mostrar subtítulos")]
    private UnityEngine.UI.Text subtitleText;

    [SerializeField]
    [Tooltip("Pistas de subtítulos")]
    private Subtitle[] subtitles;

    private int currentSubtitleIndex = -1;

    private void Update()
    {
        SyncSubtitles();
    }

    /// <summary>
    /// EN: Synchronize subtitle display with video playback time.
    /// ES: Sincronizar visualización de subtítulos con tiempo de reproducción de video.
    /// </summary>
    private void SyncSubtitles()
    {
        if (!videoPlayer.IsPlaying() || subtitleText == null)
            return;

        float currentTime = videoPlayer.GetCurrentTime();

        // EN: Find matching subtitle using LINQ / ES: Encontrar subtítulo coincidente usando LINQ
        var activeSubtitle = subtitles
            .FirstOrDefault(s => currentTime >= s.startTime && currentTime < s.endTime);

        if (activeSubtitle != null)
        {
            subtitleText.text = activeSubtitle.text;
        }
        else
        {
            subtitleText.text = "";
        }
    }

    public void LoadSubtitles(Subtitle[] newSubtitles)
    {
        subtitles = newSubtitles;
        subtitles = subtitles.OrderBy(s => s.startTime).ToArray();
    }
}

public class VideoPlayerUIController : MonoBehaviour
{
    [SerializeField] private StreamingVideoPlayer videoPlayer;
    [SerializeField] private UnityEngine.UI.Slider progressSlider;
    [SerializeField] private UnityEngine.UI.Slider volumeSlider;
    [SerializeField] private UnityEngine.UI.Button playButton;
    [SerializeField] private UnityEngine.UI.Button pauseButton;
    
    private void Start()
    {
        playButton.onClick.AddListener(() => videoPlayer.ResumeVideo());
        pauseButton.onClick.AddListener(() => videoPlayer.PauseVideo());
        progressSlider.onValueChanged.AddListener(OnProgressChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }
    
    private void Update()
    {
        // Update progress slider
        progressSlider.value = videoPlayer.GetProgress();
    }
    
    private void OnProgressChanged(float value)
    {
        float newTime = value * videoPlayer.GetDuration();
        videoPlayer.SeekToTime(newTime);
    }
    
    private void OnVolumeChanged(float value)
    {
        videoPlayer.GetComponent<AudioSource>().volume = value;
    }
}
