using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Advanced audio management system with object pooling, 3D spatial attenuation, mixer control, and fade effects.
/// ES: Sistema avanzado de gestión de audio con pooling de objetos, atenuación espacial 3D, control de mixer y efectos de desvanecimiento.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSourcePool : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Número de fuentes de audio pre-asignadas en el pool")]
    private int poolSize = 20;

    [SerializeField]
    [Tooltip("Distancia máxima para atenuación 3D")]
    private float maxDistance = 150f;

    [SerializeField]
    [Tooltip("Grupo de mixer para aplicar efectos globales")]
    private AudioMixerGroup mixerGroup;

    private Queue<AudioSource> availableSources = new Queue<AudioSource>();
    private HashSet<AudioSource> activeSources = new HashSet<AudioSource>();
    private bool poolInitialized = false;

    private void Start()
    {
        if (!poolInitialized)
            InitializePool();
    }

    /// <summary>
    /// EN: Initialize the audio source pool with pre-allocated instances.
    /// ES: Inicializa el pool de fuentes de audio con instancias pre-asignadas.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // EN: Create pooled audio source / ES: Crear fuente de audio agrupada
            GameObject sourceObj = new GameObject($"PooledAudioSource_{i}");
            sourceObj.transform.SetParent(transform);
            AudioSource source = sourceObj.AddComponent<AudioSource>();

            source.outputAudioMixerGroup = mixerGroup;
            source.spatialBlend = 0.5f;
            source.maxDistance = maxDistance;
            source.playOnAwake = false;
            sourceObj.SetActive(false);

            availableSources.Enqueue(source);
        }

        poolInitialized = true;
    }

    /// <summary>
    /// EN: Get an available audio source from the pool.
    /// ES: Obtiene una fuente de audio disponible del pool.
    /// </summary>
    public AudioSource GetAvailableSource()
    {
        AudioSource source;

        if (availableSources.Count > 0)
        {
            // EN: Reuse from pool / ES: Reutilizar del pool
            source = availableSources.Dequeue();
            source.gameObject.SetActive(true);
        }
        else
        {
            // EN: Create new if exhausted / ES: Crear nuevo si se agota
            GameObject sourceObj = new GameObject("PooledAudioSource_Extra");
            sourceObj.transform.SetParent(transform);
            source = sourceObj.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = mixerGroup;
        }

        activeSources.Add(source);
        return source;
    }

    /// <summary>
    /// EN: Return an audio source to the pool for reuse.
    /// ES: Devuelve una fuente de audio al pool para reutilización.
    /// </summary>
    public void ReturnSourceToPool(AudioSource source)
    {
        if (activeSources.Contains(source))
        {
            // EN: Stop and reset source / ES: Detener y reiniciar fuente
            source.Stop();
            source.gameObject.SetActive(false);
            activeSources.Remove(source);
            availableSources.Enqueue(source);
        }
    }
}

/// <summary>
/// EN: Main audio management system with caching, category volume control, and fade effects.
/// ES: Sistema principal de gestión de audio con almacenamiento en caché, control de volumen por categoría y efectos de desvanecimiento.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    [Tooltip("Pool de fuentes de audio disponibles")]
    private AudioSourcePool audioPool;

    [SerializeField]
    [Tooltip("Máxima distancia de atenuación 3D")]
    private float maxAudioDistance = 150f;

    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, float> categoryVolumes = new Dictionary<string, float>()
    {
        { "Music", 0.8f },
        { "SFX", 1.0f },
        { "Voice", 0.9f },
        { "Ambient", 0.6f }
    };

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// EN: Play a sound effect at a world position with 3D attenuation.
    /// ES: Reproduce un efecto de sonido en una posición mundial con atenuación 3D.
    /// </summary>
    public void PlaySound(string soundKey, Vector3 worldPosition, string category = "SFX", float volume = 1f)
    {
        // EN: Check cache / ES: Verificar caché
        if (!audioCache.ContainsKey(soundKey))
        {
            Debug.LogWarning($"Sound not cached: {soundKey}");
            return;
        }

        AudioSource source = audioPool.GetAvailableSource();
        if (source == null) return;

        // EN: Configure spatial audio / ES: Configurar audio espacial
        source.clip = audioCache[soundKey];
        source.volume = volume * categoryVolumes.GetValueOrDefault(category, 1f);
        source.spatialBlend = 1f;
        source.maxDistance = maxAudioDistance;
        source.transform.position = worldPosition;

        source.Play();

        // EN: Return to pool after playback / ES: Devolver al pool después de reproducir
        StartCoroutine(ReturnSourceAfterPlayback(source, source.clip.length));
    }

    /// <summary>
    /// EN: Play music with fade-in effect.
    /// ES: Reproduce música con efecto de entrada suave.
    /// </summary>
    public void PlayMusicWithFade(string musicKey, float fadeDuration = 2f)
    {
        if (!audioCache.ContainsKey(musicKey))
        {
            Debug.LogWarning($"Music not cached: {musicKey}");
            return;
        }

        AudioSource source = audioPool.GetAvailableSource();
        if (source == null) return;

        source.clip = audioCache[musicKey];
        source.loop = true;
        source.spatialBlend = 0f;  // EN: 2D music / ES: Música 2D
        source.volume = 0f;  // EN: Start silent / ES: Comenzar silencioso

        source.Play();
        StartCoroutine(FadeInAudio(source, fadeDuration, categoryVolumes["Music"]));
    }

    /// <summary>
    /// EN: Stop music with fade-out effect.
    /// ES: Detiene la música con efecto de salida suave.
    /// </summary>
    public void StopMusicWithFade(AudioSource source, float fadeDuration = 2f)
    {
        StartCoroutine(FadeOutAudio(source, fadeDuration));
    }

    public void CacheAudioClip(string key, AudioClip clip)
    {
        if (!audioCache.ContainsKey(key))
            audioCache[key] = clip;
    }

    public void SetCategoryVolume(string category, float volume)
    {
        if (categoryVolumes.ContainsKey(category))
            categoryVolumes[category] = Mathf.Clamp01(volume);
    }

    private IEnumerator ReturnSourceAfterPlayback(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration + 0.1f);
        audioPool.ReturnSourceToPool(source);
    }

    private IEnumerator FadeInAudio(AudioSource source, float duration, float targetVolume)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.Stop();
        audioPool.ReturnSourceToPool(source);
    }
}
