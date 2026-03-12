using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Dialog node with conditional branching and event callbacks.
/// ES: Nodo de diálogo con ramificación condicional y callbacks de eventos.
/// </summary>
[System.Serializable]
public class DialogNode
{
    public int nodeId;
    
    [Tooltip("Clave de localización para el texto del diálogo")]
    public string textKey;
    
    [Tooltip("Opciones de diálogo disponibles")]
    public List<DialogChoice> choices = new List<DialogChoice>();
    
    [Tooltip("Evento al iniciar el nodo")]
    public UnityEvent onNodeStart;
    
    [Tooltip("Evento al terminar el nodo")]
    public UnityEvent onNodeEnd;
}

/// <summary>
/// EN: Dialog choice with optional condition for branching.
/// ES: Opción de diálogo con condición opcional para ramificación.
/// </summary>
[System.Serializable]
public class DialogChoice
{
    [Tooltip("Clave de localización para el texto de la opción")]
    public string choiceTextKey;
    
    [Tooltip("ID del siguiente nodo")]
    public int nextNodeId;
    
    [HideInInspector] public System.Func<bool> condition;
}

/// <summary>
/// EN: Localization manager for multi-language dialog support.
/// ES: Gestor de localización para soporte multiidioma.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    
    private Dictionary<string, Dictionary<string, string>> localizations;
    private string currentLanguage = "EN";
    
    [Tooltip("Idiomas disponibles")]
    [SerializeField] private List<string> supportedLanguages = new List<string> { "EN", "ES" };
    
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }
    
    /// <summary>
    /// EN: Get localized text by key.
    /// ES: Obtener texto localizado por clave.
    /// </summary>
    public string GetLocalizedText(string key)
    {
        if (!localizations.ContainsKey(currentLanguage))
            return key;
        
        var texts = localizations[currentLanguage];
        return texts.ContainsKey(key) ? texts[key] : key;
    }
    
    /// <summary>
    /// EN: Set active language and reload catalog.
    /// ES: Establecer idioma activo y recargar catálogo.
    /// </summary>
    public void SetLanguage(string language)
    {
        if (supportedLanguages.Contains(language))
        {
            currentLanguage = language;
            LoadCatalog(language);
        }
    }
    
    /// <summary>EN: Load catalog for specific language. ES: Cargar catálogo del idioma.</summary>
    private void LoadCatalog(string language)
    {
        // EN: Load from resources or database
        // ES: Cargar desde recursos o base de datos
        Debug.Log($"Loaded {language} catalog");
    }
    
    public string CurrentLanguage => currentLanguage;
}

/// <summary>
/// EN: Main dialog system with branching narrative using coroutines.
/// ES: Sistema de diálogo principal con narrativa ramificada usando corrutinas.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DialogSystem : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Gestor de localización")]
    private LocalizationManager localizationManager;
    
    private Dictionary<int, DialogNode> dialogNodes = new Dictionary<int, DialogNode>();
    private DialogNode currentNode;
    private int currentNodeId = 0;
    
    [HideInInspector] public bool onDialog = false;
    [HideInInspector] public Coroutine dialogCoroutine;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InitializeDialogTree();
    }
    
    /// <summary>
    /// EN: Initialize dialog tree with sample nodes.
    /// ES: Inicializar árbol de diálogos con nodos de ejemplo.
    /// </summary>
    private void InitializeDialogTree()
    {
        // EN: Create node 0 with branching choices
        // ES: Crear nodo 0 con opciones ramificadas
        var node0 = new DialogNode
        {
            nodeId = 0,
            textKey = "dialog_greeting",
            choices = new List<DialogChoice>
            {
                new DialogChoice 
                { 
                    choiceTextKey = "choice_help",
                    nextNodeId = 1,
                    condition = () => true
                },
                new DialogChoice 
                { 
                    choiceTextKey = "choice_story",
                    nextNodeId = 2,
                    condition = () => HasUnlockedStory()
                }
            }
        };
        
        dialogNodes.Add(0, node0);
    }
    
    /// <summary>
    /// EN: Start dialog at specific node using coroutine.
    /// ES: Iniciar diálogo en nodo específico usando corrutina.
    /// </summary>
    public void StartDialog(int nodeId = 0)
    {
        if (onDialog) return;
        
        onDialog = true;
        currentNodeId = nodeId;
        
        if (dialogCoroutine != null)
            StopCoroutine(dialogCoroutine);
        
        dialogCoroutine = StartCoroutine(PlayDialogSequence());
    }
    
    /// <summary>
    /// EN: Dialog sequence coroutine with node transitions.
    /// ES: Corrutina de secuencia de diálogo con transiciones.
    /// </summary>
    private IEnumerator PlayDialogSequence()
    {
        while (onDialog && dialogNodes.TryGetValue(currentNodeId, out currentNode))
        {
            // EN: Invoke node start event
            // ES: Invocar evento de inicio de nodo
            currentNode.onNodeStart?.Invoke();
            
            string localizedText = localizationManager.GetLocalizedText(currentNode.textKey);
            OnDialogNodeDisplayed(localizedText);
            
            // EN: Wait for player choice or timeout
            // ES: Esperar opción del jugador o timeout
            yield return new WaitForSeconds(0.5f);
            
            currentNode.onNodeEnd?.Invoke();
            
            onDialog = false;
        }
    }
    
    /// <summary>
    /// EN: Get available choices with condition filtering using LINQ.
    /// ES: Obtener opciones disponibles filtrando condiciones con LINQ.
    /// </summary>
    public List<DialogChoice> GetAvailableChoices()
    {
        if (currentNode == null)
            return new List<DialogChoice>();
        
        return currentNode.choices
            .Where(c => c.condition == null || c.condition.Invoke())
            .ToList();
    }
    
    /// <summary>
    /// EN: Select dialog choice and transition to next node.
    /// ES: Seleccionar opción de diálogo y transicionar al siguiente nodo.
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        var availableChoices = GetAvailableChoices();
        
        if (choiceIndex < 0 || choiceIndex >= availableChoices.Count)
            return;
        
        var choice = availableChoices[choiceIndex];
        currentNodeId = choice.nextNodeId;
        
        string choiceText = localizationManager.GetLocalizedText(choice.choiceTextKey);
        OnChoiceSelected(choiceText);
        
        if (onDialog && dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
            dialogCoroutine = StartCoroutine(PlayDialogSequence());
        }
    }
    
    /// <summary>EN: Check if story is unlocked. ES: Verificar si la historia está desbloqueada.</summary>
    private bool HasUnlockedStory() => true; // EN: Replace with actual game state check
    
    /// <summary>EN: Callback when node text is displayed. ES: Callback cuando se muestra texto de nodo.</summary>
    private void OnDialogNodeDisplayed(string text)
    {
        Debug.Log($"Dialog: {text}");
    }
    
    /// <summary>EN: Callback when player selects choice. ES: Callback cuando jugador selecciona opción.</summary>
    private void OnChoiceSelected(string choiceText)
    {
        Debug.Log($"Selected: {choiceText}");
    }
}
