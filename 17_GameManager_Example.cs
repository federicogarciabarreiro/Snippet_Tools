using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Central game manager handling state machine, scene lifecycle, persistence, and subsystem coordination.
/// ES: Administrador central del juego que maneja la máquina de estados, ciclo de vida de escenas, persistencia y coordinación de subsistemas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [System.Serializable]
    public class GameData
    {
        public string session_id = System.Guid.NewGuid().ToString();
        public int currentLevel = 1;
        public float totalPlayTime = 0f;
        public int totalScore = 0;
        public System.DateTime lastSaveTime;
    }

    public enum GameState { Menu, Loading, Gameplay, Paused, GameOver }

    public GameState CurrentState { get; private set; }

    [SerializeField]
    [Tooltip("Tiempo máximo para cargar una escena")]
    private float sceneLoadTimeout = 30f;

    [SerializeField]
    [Tooltip("Guardar datos del juego automáticamente")]
    private bool autoSaveEnabled = true;

    [SerializeField]
    [Tooltip("Intervalo de guardado automático en segundos")]
    private float autoSaveInterval = 60f;

    private GameState nextState;
    private Dictionary<GameState, System.Action> stateEnterCallbacks;
    private Dictionary<GameState, System.Action> stateExitCallbacks;
    private GameData gameData;
    private bool isLoadingScene = false;
    private float timeSinceLastSave = 0f;

    private void Awake()
    {
        // EN: Implement singleton pattern / ES: Implementar patrón singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // EN: Initialize game manager / ES: Inicializar administrador de juego
        InitializeStateCallbacks();
        LoadGameData();
        EnterState(GameState.Menu);
    }

    /// <summary>
    /// EN: Initialize state transition callbacks for all game states.
    /// ES: Inicializa callbacks de transición de estado para todos los estados del juego.
    /// </summary>
    private void InitializeStateCallbacks()
    {
        stateEnterCallbacks = new Dictionary<GameState, System.Action>
        {
            { GameState.Menu, OnEnterMenu },
            { GameState.Loading, OnEnterLoading },
            { GameState.Gameplay, OnEnterGameplay },
            { GameState.Paused, OnEnterPaused },
            { GameState.GameOver, OnEnterGameOver }
        };

        stateExitCallbacks = new Dictionary<GameState, System.Action>
        {
            { GameState.Menu, OnExitMenu },
            { GameState.Loading, OnExitLoading },
            { GameState.Gameplay, OnExitGameplay },
            { GameState.Paused, OnExitPaused },
            { GameState.GameOver, OnExitGameOver }
        };
    }

    private void Update()
    {
        HandleStateTransitions();
        UpdateAutoSave();
    }

    /// <summary>
    /// EN: Request state transition.
    /// ES: Solicita transición de estado.
    /// </summary>
    public void TransitionToState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        nextState = newState;
    }

    private void HandleStateTransitions()
    {
        // EN: Process pending state transition / ES: Procesar transición de estado pendiente
        if (nextState != CurrentState)
        {
            ExitState(CurrentState);
            EnterState(nextState);
        }
    }

    /// <summary>
    /// EN: Enter a new game state and execute its callback.
    /// ES: Entra en un nuevo estado del juego y ejecuta su callback.
    /// </summary>
    private void EnterState(GameState state)
    {
        CurrentState = state;

        if (stateEnterCallbacks.ContainsKey(state))
            stateEnterCallbacks[state].Invoke();

        Debug.Log($"Game state: {state}");
    }

    /// <summary>
    /// EN: Exit current game state and execute its exit callback.
    /// ES: Sale del estado actual del juego y ejecuta su callback de salida.
    /// </summary>
    private void ExitState(GameState state)
    {
        if (stateExitCallbacks.ContainsKey(state))
            stateExitCallbacks[state].Invoke();
    }

    // EN: State callback implementations / ES: Implementaciones de callbacks de estado
    private void OnEnterMenu() 
    { 
        Time.timeScale = 1f;
        Debug.Log("Entering menu");
    }
    private void OnExitMenu() { }

    private void OnEnterLoading()
    {
        Time.timeScale = 0f;  // EN: Pause during loading / ES: Pausar durante carga
        Debug.Log("Entering loading state");
    }
    private void OnExitLoading() { }

    private void OnEnterGameplay()
    {
        Time.timeScale = 1f;
        Debug.Log("Entering gameplay");
    }

    private void OnExitGameplay()
    {
        SaveGameData();
    }

    private void OnEnterPaused()
    {
        Time.timeScale = 0f;
        Debug.Log("Game paused");
    }

    private void OnExitPaused()
    {
        Time.timeScale = 1f;
    }

    private void OnEnterGameOver()
    {
        Time.timeScale = 0f;
        Debug.Log("Game over");
    }

    private void OnExitGameOver() { }

    /// <summary>
    /// EN: Load a scene asynchronously with timeout protection.
    /// ES: Carga una escena de forma asincrónica con protección de tiempo de espera.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isLoadingScene)
            return;

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoadingScene = true;
        TransitionToState(GameState.Loading);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        float timer = 0f;

        // EN: Wait for scene load with timeout / ES: Esperar carga de escena con tiempo de espera
        while (!asyncLoad.isDone && timer < sceneLoadTimeout)
        {
            Debug.Log($"Loading {sceneName}: {asyncLoad.progress * 100:F0}%");
            timer += Time.deltaTime;
            yield return null;
        }

        isLoadingScene = false;

        if (asyncLoad.isDone)
        {
            TransitionToState(GameState.Gameplay);
            Debug.Log($"Scene loaded: {sceneName}");
        }
        else
        {
            Debug.LogError($"Scene load timeout: {sceneName}");
            TransitionToState(GameState.GameOver);
        }
    }

    /// <summary>
    /// EN: Load game data from PlayerPrefs or create new data.
    /// ES: Cargar datos del juego desde PlayerPrefs o crear nuevos datos.
    /// </summary>
    private void LoadGameData()
    {
        string json = PlayerPrefs.GetString("GameData", "");
        
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                gameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"Game data loaded for session: {gameData.session_id}");
            }
            catch
            {
                Debug.LogWarning("Failed to load game data, creating new");
                gameData = new GameData();
            }
        }
        else
        {
            gameData = new GameData();
        }
    }

    /// <summary>
    /// EN: Save game data to PlayerPrefs.
    /// ES: Guardar datos del juego en PlayerPrefs.
    /// </summary>
    private void SaveGameData()
    {
        gameData.lastSaveTime = System.DateTime.Now;
        string json = JsonUtility.ToJson(gameData);
        PlayerPrefs.SetString("GameData", json);
        PlayerPrefs.Save();

        Debug.Log($"Game data saved. Session: {gameData.session_id}");
    }

    /// <summary>
    /// EN: Update auto-save timer and perform save if interval elapsed.
    /// ES: Actualizar temporizador de guardado automático y realizar guardado si transcurrió intervalo.
    /// </summary>
    private void UpdateAutoSave()
    {
        if (!autoSaveEnabled || CurrentState != GameState.Gameplay)
            return;

        timeSinceLastSave += Time.deltaTime;

        if (timeSinceLastSave >= autoSaveInterval)
        {
            SaveGameData();
            timeSinceLastSave = 0f;
        }
    }

    public GameData GetGameData() => gameData;

    /// <summary>
    /// EN: Add points to the player score.
    /// ES: Agregar puntos a la puntuación del jugador.
    /// </summary>
    public void AddScore(int points)
    {
        gameData.totalScore += points;
    }

    /// <summary>
    /// EN: Advance to next level.
    /// ES: Avanzar al siguiente nivel.
    /// </summary>
    public void NextLevel()
    {
        gameData.currentLevel++;
        LoadScene($"Level_{gameData.currentLevel}");
    }

    // EN: Application lifecycle handlers / ES: Manejadores de ciclo de vida de aplicación
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused && autoSaveEnabled)
            SaveGameData();
    }
}
