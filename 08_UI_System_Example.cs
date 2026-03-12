using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Two-way data binding property for automatic UI updates.
/// ES: Propiedad de binding bidireccional para actualizaciones automáticas de UI.
/// </summary>
[System.Serializable]
public class BindableProperty<T>
{
    private T value;
    
    public event Action<T> OnValueChanged;
    
    public T Value
    {
        get => value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(this.value, value))
            {
                this.value = value;
                OnValueChanged?.Invoke(value);
            }
        }
    }
    
    /// <summary>EN: Constructor with initial value. ES: Constructor con valor inicial.</summary>
    public BindableProperty(T initialValue = default)
    {
        this.value = initialValue;
    }
}

/// <summary>
/// EN: UI model with bindable properties for reactive UI.
/// ES: Modelo de UI con propiedades vinculables para UI reactivo.
/// </summary>
public class UIModel
{
    public BindableProperty<string> PlayerName = new BindableProperty<string>();
    public BindableProperty<int> Health = new BindableProperty<int>();
    public BindableProperty<float> Experience = new BindableProperty<float>();
    public BindableProperty<int> Score = new BindableProperty<int>();
}

/// <summary>
/// EN: UI data binder connecting model properties to UI elements using LINQ.
/// ES: Vinculador de datos de UI conectando propiedades a elementos usando LINQ.
/// </summary>
public class UIDataBinder : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Texto del nombre del jugador")]
    private Text playerNameText;
    
    [SerializeField]
    [Tooltip("Barra de salud")]
    private Image healthBar;
    
    [SerializeField]
    [Tooltip("Texto de salud")]
    private Text healthText;
    
    [SerializeField]
    [Tooltip("Barra de experiencia")]
    private Image experienceBar;
    
    private UIModel model;
    private bool isBound = false;
    
    private void Start()
    {
        model = new UIModel();
        BindUI();
        isBound = true;
        
        // EN: Simulate data changes
        // ES: Simular cambios de datos
        model.PlayerName.Value = "Hero";
        model.Health.Value = 75;
        model.Experience.Value = 0.6f;
    }
    
    /// <summary>
    /// EN: Bind UI elements to model properties reactively.
    /// ES: Vincular elementos de UI a propiedades del modelo reactivamente.
    /// </summary>
    private void BindUI()
    {
        // EN: Bind player name with string formatting
        // ES: Vincular nombre del jugador con formato de string
        model.PlayerName.OnValueChanged += (name) =>
        {
            playerNameText.text = $"Hero: {name}";
        };
        
        // EN: Bind health with percentage calculation
        // ES: Vincular salud con cálculo de porcentaje
        model.Health.OnValueChanged += (health) =>
        {
            healthBar.fillAmount = health / 100f;
            healthText.text = $"{health}/100";
        };
        
        // EN: Bind experience progress
        // ES: Vincular progreso de experiencia
        model.Experience.OnValueChanged += (exp) =>
        {
            experienceBar.fillAmount = Mathf.Clamp01(exp);
        };
    }
    
    /// <summary>EN: Update all model properties at once. ES: Actualizar todas las propiedades.</summary>
    public void UpdateModel(string playerName, int health, float exp)
    {
        model.PlayerName.Value = playerName;
        model.Health.Value = health;
        model.Experience.Value = exp;
    }
}

/// <summary>
/// EN: Responsive layout manager adjusting to device screen size.
/// ES: Gestor de layout responsivo ajustando al tamaño de pantalla.
/// </summary>
public class ResponsiveLayoutManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Escalador de canvas")]
    private CanvasScaler canvasScaler;
    
    [SerializeField]
    [Tooltip("Layout para móvil")]
    private RectTransform mobileLayout;
    
    [SerializeField]
    [Tooltip("Layout para escritorio")]
    private RectTransform desktopLayout;
    
    private bool isMobile = false;
    
    private void Start()
    {
        AdjustLayoutForDevice();
    }
    
    /// <summary>
    /// EN: Detect device type and adjust UI layout accordingly.
    /// ES: Detectar tipo de dispositivo y ajustar layout de UI.
    /// </summary>
    private void AdjustLayoutForDevice()
    {
        // EN: Detect mobile platform
        // ES: Detectar plataforma móvil
        isMobile = Application.platform == RuntimePlatform.Android || 
                   Application.platform == RuntimePlatform.IPhonePlayer;
        
        if (isMobile)
        {
            mobileLayout.gameObject.SetActive(true);
            desktopLayout.gameObject.SetActive(false);
            
            // EN: Configure canvas for mobile resolution
            // ES: Configurar canvas para resolución móvil
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
        }
        else
        {
            mobileLayout.gameObject.SetActive(false);
            desktopLayout.gameObject.SetActive(true);
            
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
        }
        
        Debug.Log($"Layout adjusted for {(isMobile ? "Mobile" : "Desktop")}");
    }
    
    /// <summary>EN: Get current layout status. ES: Obtener estado layout actual.</summary>
    public bool IsMobileLayout() => isMobile;
}

/// <summary>
/// EN: Dynamic theme manager applying colors to UI elements using LINQ.
/// ES: Gestor de tema dinámico aplicando colores con LINQ.
/// </summary>
public class DynamicThemeManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Color primario del tema")]
    private Color primaryColor = Color.blue;
    
    [SerializeField]
    [Tooltip("Color secundario del tema")]
    private Color secondaryColor = Color.green;
    
    private Image[] allImages;
    
    private void Start()
    {
        // EN: Cache all image components
        // ES: Cachear todos los componentes Image
        allImages = FindObjectsOfType<Image>();
        ApplyTheme();
    }
    
    /// <summary>
    /// EN: Change theme colors and reapply to all UI.
    /// ES: Cambiar colores de tema y reaplicar a toda la UI.
    /// </summary>
    public void ChangeTheme(Color primary, Color secondary)
    {
        primaryColor = primary;
        secondaryColor = secondary;
        ApplyTheme();
    }
    
    /// <summary>
    /// EN: Apply current theme to all UI elements using LINQ grouping.
    /// ES: Aplicar tema a todos los elementos usando agrupación LINQ.
    /// </summary>
    private void ApplyTheme()
    {
        // EN: Use LINQ to group and filter images by tag
        // ES: Usar LINQ para agrupar y filtrar imágenes por tag
        var primaryButtons = allImages.Where(img => img.CompareTag("PrimaryButton")).ToList();
        var secondaryButtons = allImages.Where(img => img.CompareTag("SecondaryButton")).ToList();
        
        primaryButtons.ForEach(img => img.color = primaryColor);
        secondaryButtons.ForEach(img => img.color = secondaryColor);
        
        Debug.Log($"Theme applied to {primaryButtons.Count} primary and {secondaryButtons.Count} secondary buttons");
    }
}
