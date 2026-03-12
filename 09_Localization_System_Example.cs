using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// EN: Localization manager with multi-language support, RTL, and parameter substitution.
/// ES: Gestor de localización multidioma con soporte RTL y sustitución de parámetros.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [SerializeField]
    [Tooltip("Idioma actual (EN, ES, AR, etc.)")]
    private string currentLanguage = "EN";

    private Dictionary<string, Dictionary<string, string>> catalogs;
    private Dictionary<string, bool> languageIsRTL = new Dictionary<string, bool>
    {
        { "EN", false },
        { "ES", false },
        { "AR", true },   // EN: Arabic (Right-To-Left) / ES: Árabe (Derecha-a-Izquierda)
        { "HE", true }    // EN: Hebrew (Right-To-Left) / ES: Hebreo (Derecha-a-Izquierda)
    };

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        LoadCatalog(currentLanguage);
    }

    /// <summary>
    /// EN: Load localization catalog for specified language.
    /// ES: Carga el catálogo de localización para el idioma especificado.
    /// </summary>
    public void LoadCatalog(string language)
    {
        currentLanguage = language;
        catalogs = new Dictionary<string, Dictionary<string, string>>();

        // EN: In real implementation, load from JSON/CSV files / ES: En implementación real, cargar desde JSON/CSV
        catalogs.Add("EN", new Dictionary<string, string>
        {
            { "hello", "Hello" },
            { "welcome", "Welcome, {0}" },
            { "items_count", "You have {0} items" }
        });

        catalogs.Add("ES", new Dictionary<string, string>
        {
            { "hello", "Hola" },
            { "welcome", "Bienvenido, {0}" },
            { "items_count", "Tienes {0} artículos" }
        });

        catalogs.Add("AR", new Dictionary<string, string>
        {
            { "hello", "مرحبا" },
            { "welcome", "أهلا وسهلا، {0}" },
            { "items_count", "لديك {0} عنصر" }
        });

        AdjustUIForLanguage();
        Debug.Log($"Localization loaded for language: {language}");
    }

    /// <summary>
    /// EN: Get localized text with optional parameter substitution.
    /// ES: Obtiene texto localizado con sustitución opcional de parámetros.
    /// </summary>
    public string GetText(string key, params object[] parameters)
    {
        if (!catalogs.ContainsKey(currentLanguage))
            return key;

        var catalog = catalogs[currentLanguage];
        if (!catalog.ContainsKey(key))
            return key;

        string text = catalog[key];

        // EN: Substitute parameters / ES: Sustituir parámetros
        if (parameters.Length > 0)
            text = string.Format(text, parameters);

        return text;
    }

    /// <summary>
    /// EN: Check if current language uses right-to-left (RTL) layout.
    /// ES: Verifica si el idioma actual usa diseño de derecha a izquierda (RTL).
    /// </summary>
    public bool IsCurrentLanguageRTL()
    {
        return languageIsRTL.ContainsKey(currentLanguage) && languageIsRTL[currentLanguage];
    }

    /// <summary>
    /// EN: Adjust UI layout direction based on current language (RTL/LTR).
    /// ES: Ajusta la dirección del diseño de UI según el idioma actual (RTL/LTR).
    /// </summary>
    private void AdjustUIForLanguage()
    {
        bool isRTL = IsCurrentLanguageRTL();

        // EN: Find all UI text elements and adjust layout / ES: Encontrar todos los elementos de texto UI y ajustar diseño
        Text[] allTexts = FindObjectsOfType<Text>();
        allTexts.ForEach(text =>
        {
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (isRTL)
                {
                    rectTransform.localScale = new Vector3(-1, 1, 1);
                    text.alignment = TextAnchor.UpperRight;
                }
                else
                {
                    rectTransform.localScale = Vector3.one;
                    text.alignment = TextAnchor.UpperLeft;
                }
            }
        });
    }

    /// <summary>
    /// EN: Get available languages in the system.
    /// ES: Obtiene idiomas disponibles en el sistema.
    /// </summary>
    public List<string> GetAvailableLanguages()
    {
        return catalogs.Keys.ToList();
    }

    public string GetCurrentLanguage() => currentLanguage;
}

/// <summary>
/// EN: UI Text component wrapper for automatic localization.
/// ES: Envoltorio del componente Text de UI para localización automática.
/// </summary>
public class UITextLocalizer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Clave de texto para buscar en el catálogo")]
    private string textKey;

    [SerializeField]
    [Tooltip("Usar parámetros dinámicos para sustitución")]
    private bool useParameters = false;

    [SerializeField]
    [Tooltip("Claves de parámetros a sustituir")]
    private string[] parameterKeys;

    private Text textComponent;

    private void Start()
    {
        textComponent = GetComponent<Text>();
        UpdateText();
    }

    /// <summary>
    /// EN: Update UI text with localized value.
    /// ES: Actualizar texto de UI con valor localizado.
    /// </summary>
    private void UpdateText()
    {
        if (useParameters && parameterKeys.Length > 0)
        {
            // EN: Substitute parameters / ES: Sustituir parámetros
            object[] parameters = parameterKeys.Select(k => (object)k).ToArray();
            textComponent.text = LocalizationManager.Instance.GetText(textKey, parameters);
        }
        else
        {
            textComponent.text = LocalizationManager.Instance.GetText(textKey);
        }
    }
}

/// <summary>
/// EN: Handle plural forms based on language and quantity rules.
/// ES: Manejar formas plurales basadas en reglas de idioma y cantidad.
/// </summary>
public class PluralHandler
{
    /// <summary>
    /// EN: Get appropriate plural form for specified language and count.
    /// ES: Obtener forma plural apropiada para idioma y cantidad especificados.
    /// </summary>
    public static string GetPlural(string key, int count, string language)
    {
        // EN: Different languages have different plural rules / ES: Diferentes idiomas tienen diferentes reglas de plural
        if (language == "EN")
        {
            return count == 1 ? "item" : "items";
        }
        else if (language == "ES")
        {
            return count == 1 ? "artículo" : "artículos";
        }
        else if (language == "AR")
        {
            // EN: Arabic has complex pluralization rules / ES: Árabe tiene reglas de pluralización complejas
            if (count == 0) return "عناصر";  // zero
            if (count == 1) return "عنصر";   // singular
            if (count == 2) return "عنصرين"; // dual
            return "عناصر";                  // plural (3+)
        }

        return "items";
    }
}
