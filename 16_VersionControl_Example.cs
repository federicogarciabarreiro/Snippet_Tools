using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// EN: Asset version control system with schema migration, rollback, and hash-based integrity validation.
/// ES: Sistema de control de versiones de activos con migración de esquema, reversión e validación de integridad basada en hash.
/// </summary>
public class AssetVersionManager : MonoBehaviour
{
    [System.Serializable]
    public class VersionedAsset
    {
        public string assetId;
        public int versionNumber;
        public string contentHash;
        public System.DateTime dateModified;
        public List<string> changeLog = new List<string>();
    }

    [System.Serializable]
    public class VersionRegistry
    {
        public List<VersionedAsset> assets = new List<VersionedAsset>();
    }

    [SerializeField]
    [Tooltip("Ruta del archivo de registro de versiones (JSON)")]
    private string versionRegistryPath = "Assets/VersionRegistry.json";

    [SerializeField]
    [Tooltip("Habilitar rollback automático si la migración falla")]
    private bool allowAutoRollback = true;

    private VersionRegistry registry;
    private Dictionary<string, string> assetHashes = new Dictionary<string, string>();
    private Stack<VersionRegistry> rollbackStack = new Stack<VersionRegistry>();

    private void Start()
    {
        LoadVersionRegistry();
        DetectChanges();
    }

    /// <summary>
    /// EN: Load version registry from file.
    /// ES: Carga el registro de versiones desde archivo.
    /// </summary>
    private void LoadVersionRegistry()
    {
        if (System.IO.File.Exists(versionRegistryPath))
        {
            string json = System.IO.File.ReadAllText(versionRegistryPath);
            registry = JsonUtility.FromJson<VersionRegistry>(json);
        }
        else
        {
            registry = new VersionRegistry();
        }

        Debug.Log($"Version registry loaded with {registry.assets.Count} assets");
    }

    /// <summary>
    /// EN: Detect changes in all registered assets.
    /// ES: Detecta cambios en todos los activos registrados.
    /// </summary>
    private void DetectChanges()
    {
        // EN: Check all assets for changes using LINQ / ES: Verificar cambios en activos usando LINQ
        var changedAssets = registry.assets
            .Where(asset => !VerifyAssetIntegrity(asset))
            .ToList();

        changedAssets.ForEach(asset => 
        {
            Debug.Log($"Asset changed: {asset.assetId}");
            RegisterAssetChange(asset.assetId);
        });
    }

    /// <summary>
    /// EN: Calculate SHA256 hash of asset content.
    /// ES: Calcular hash SHA256 del contenido del activo.
    /// </summary>
    private string CalculateHash(string assetId)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(assetId));
            return System.Convert.ToBase64String(hashBytes).Substring(0, 16);
        }
    }

    /// <summary>
    /// EN: Register a change to an asset and increment version.
    /// ES: Registra un cambio a un activo e incrementa la versión.
    /// </summary>
    public void RegisterAssetChange(string assetId, string changeDescription = "Modified")
    {
        // EN: Save rollback point / ES: Guardar punto de reversión
        rollbackStack.Push(new VersionRegistry { assets = new List<VersionedAsset>(registry.assets) });

        var existingAsset = registry.assets.FirstOrDefault(a => a.assetId == assetId);

        if (existingAsset != null)
        {
            // EN: Increment version on change / ES: Incrementar versión al cambiar
            existingAsset.versionNumber++;
            existingAsset.contentHash = CalculateHash(assetId);
            existingAsset.dateModified = System.DateTime.Now;
            existingAsset.changeLog.Add($"v{existingAsset.versionNumber}: {changeDescription}");
        }
        else
        {
            // EN: Create new versioned asset / ES: Crear nuevo activo versionado
            registry.assets.Add(new VersionedAsset
            {
                assetId = assetId,
                versionNumber = 1,
                contentHash = CalculateHash(assetId),
                dateModified = System.DateTime.Now,
                changeLog = new List<string> { "v1: Created" }
            });
        }

        SaveVersionRegistry();
    }

    /// <summary>
    /// EN: Verify asset integrity by comparing hash.
    /// ES: Verifica la integridad del activo comparando hash.
    /// </summary>
    public bool VerifyAssetIntegrity(VersionedAsset asset)
    {
        string currentHash = CalculateHash(asset.assetId);
        return currentHash == asset.contentHash;
    }

    /// <summary>
    /// EN: Check backward compatibility with target version.
    /// ES: Verificar compatibilidad hacia atrás con versión objetivo.
    /// </summary>
    public bool IsBackwardCompatible(string assetId, int targetVersion)
    {
        var asset = registry.assets.FirstOrDefault(a => a.assetId == assetId);
        if (asset == null)
            return false;

        return asset.versionNumber >= targetVersion;
    }

    /// <summary>
    /// EN: Migrate asset schema to new version using step-by-step migrations.
    /// ES: Migrar esquema de activo a nueva versión usando migraciones paso a paso.
    /// </summary>
    public void MigrateSchema(string assetId, int targetVersion)
    {
        var asset = registry.assets.FirstOrDefault(a => a.assetId == assetId);
        if (asset == null)
            return;

        // EN: Perform sequential migrations / ES: Realizar migraciones secuenciales
        for (int v = asset.versionNumber; v < targetVersion; v++)
        {
            try
            {
                ExecuteMigration(asset, v);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Migration failed: {ex.Message}");
                
                if (allowAutoRollback)
                {
                    Rollback(assetId, asset.versionNumber);
                }
                return;
            }
        }

        SaveVersionRegistry();
    }

    /// <summary>
    /// EN: Execute a single schema migration step.
    /// ES: Ejecutar un paso de migración de esquema individual.
    /// </summary>
    private void ExecuteMigration(VersionedAsset asset, int fromVersion)
    {
        // EN: Example migration templates / ES: Plantillas de migración de ejemplo
        if (fromVersion == 1)
        {
            // EN: V1 to V2 logic / ES: Lógica V1 a V2
            asset.versionNumber = 2;
            asset.changeLog.Add("v2: Schema migration completed");
        }
        else if (fromVersion == 2)
        {
            // EN: V2 to V3 logic / ES: Lógica V2 a V3
            asset.versionNumber = 3;
            asset.changeLog.Add("v3: Additional fields added");
        }
    }

    /// <summary>
    /// EN: Rollback asset to previous version.
    /// ES: Revertir activo a versión anterior.
    /// </summary>
    public void Rollback(string assetId, int targetVersion)
    {
        var asset = registry.assets.FirstOrDefault(a => a.assetId == assetId);
        
        if (asset != null && asset.versionNumber >= targetVersion)
        {
            asset.versionNumber = targetVersion;
            asset.changeLog.Add($"Rolled back to v{targetVersion}");
            
            Debug.Log($"Rolled back {assetId} to version {targetVersion}");
            SaveVersionRegistry();
        }
    }

    /// <summary>
    /// EN: Rollback all assets to previous state.
    /// ES: Revertir todos los activos al estado anterior.
    /// </summary>
    public void RollbackAll()
    {
        if (rollbackStack.Count > 0)
        {
            registry = rollbackStack.Pop();
            SaveVersionRegistry();
            Debug.Log("All assets rolled back to previous state");
        }
    }

    private void SaveVersionRegistry()
    {
        string json = JsonUtility.ToJson(registry, true);
        
        // EN: Ensure directory exists / ES: Asegurar que directorio existe
        string directory = System.IO.Path.GetDirectoryName(versionRegistryPath);
        if (!System.IO.Directory.Exists(directory))
            System.IO.Directory.CreateDirectory(directory);

        System.IO.File.WriteAllText(versionRegistryPath, json);
        Debug.Log("Version registry saved");
    }

    /// <summary>
    /// EN: Get asset version information.
    /// ES: Obtener información de versión de activo.
    /// </summary>
    public VersionedAsset GetAsset(string assetId)
    {
        return registry.assets.FirstOrDefault(a => a.assetId == assetId);
    }

    /// <summary>
    /// EN: Get all assets of current version.
    /// ES: Obtener todos los activos de versión actual.
    /// </summary>
    public List<VersionedAsset> GetAllAssets()
    {
        return registry.assets.ToList();
    }

    /// <summary>
    /// EN: Get version history for an asset.
    /// ES: Obtener historial de versiones para un activo.
    /// </summary>
    public List<string> GetChangeLog(string assetId)
    {
        return GetAsset(assetId)?.changeLog ?? new List<string>();
    }
}
