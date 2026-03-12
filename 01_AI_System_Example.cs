using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// EN: AI Dialog System with Vector Embeddings and contextual response generation.
/// Uses embeddings cache for performance optimization with LINQ queries.
/// ES: Sistema de Diálogo IA con embeddings vectoriales y generación de respuestas contextuales.
/// Utiliza caché de embeddings con optimización LINQ.
/// </summary>
public interface IDialogSystem
{
    /// <summary>EN: Generate contextual response. ES: Generar respuesta contextual.</summary>
    Task<string> GenerateResponseAsync(string userInput);
    
    /// <summary>EN: Calculate cosine similarity between vectors. ES: Calcular similitud coseno.</summary>
    float CalculateSimilarity(float[] vector1, float[] vector2);
}

/// <summary>
/// EN: Cache system for embedding vectors with LINQ optimization.
/// ES: Sistema de caché para vectores de embedding con optimización LINQ.
/// </summary>
public class EmbeddingCache
{
    private Dictionary<string, float[]> cache = new Dictionary<string, float[]>();
    
    /// <summary>EN: Try retrieve embedding from cache. ES: Intentar obtener embedding del caché.</summary>
    public bool TryGetEmbedding(string text, out float[] embedding)
    {
        return cache.TryGetValue(text, out embedding);
    }
    
    /// <summary>EN: Store embedding with deduplication. ES: Almacenar embedding sin duplicados.</summary>
    public void StoreEmbedding(string text, float[] embedding)
    {
        if (!cache.ContainsKey(text))
            cache.Add(text, embedding);
    }
    
    /// <summary>EN: Get cached size. ES: Obtener cantidad en caché.</summary>
    public int CacheSize => cache.Count;
    
    /// <summary>EN: Get all cached keys using LINQ. ES: Obtener todas las claves con LINQ.</summary>
    public IEnumerable<string> GetCachedKeys() => cache.Keys.AsEnumerable();
    
    /// <summary>EN: Clear entire cache. ES: Limpiar todo el caché.</summary>
    public void ClearCache() => cache.Clear();
}

public class ContextualDialogSystem : MonoBehaviour, IDialogSystem
{
    [SerializeField] 
    [Tooltip("Dimensión del vector de embedding (64-512)")]
    private int embeddingDimension = 128;
    
    [SerializeField]
    [Tooltip("Umbral mínimo de similitud para respuestas")]
    private float similarityThreshold = 0.3f;
    
    private EmbeddingCache embeddingCache;
    private List<string> responseDatabase;
    private bool isInitialized = false;
    
    private void Start()
    {
        embeddingCache = new EmbeddingCache();
        InitializeResponseDatabase();
        isInitialized = true;
    }
    
    /// <summary>
    /// EN: Initialize response database with predefined responses.
    /// ES: Inicializar base de datos con respuestas predefinidas.
    /// </summary>
    private void InitializeResponseDatabase()
    {
        responseDatabase = new List<string>
        {
            "Hello, how can I help you?",
            "That's an interesting question.",
            "I understand your concern.",
            "Let me think about that...",
            "Could you elaborate on that?",
            "That's a great point."
        };
    }
    
    /// <summary>
    /// EN: Vectorize text input using deterministic hash-based approach.
    /// ES: Vectorizar entrada de texto con enfoque basado en hash.
    /// </summary>
    private float[] VectorizeInput(string text)
    {
        if (embeddingCache.TryGetEmbedding(text, out var cached))
            return cached;
        
        float[] embedding = new float[embeddingDimension];
        int seed = text.GetHashCode();
        System.Random rng = new System.Random(seed);
        
        for (int i = 0; i < embeddingDimension; i++)
            embedding[i] = (float)(rng.NextDouble() * 2f - 1f);
        
        embeddingCache.StoreEmbedding(text, embedding);
        return embedding;
    }
    
    /// <summary>
    /// EN: Calculate cosine similarity between two vectors.
    /// ES: Calcular similitud coseno entre dos vectores.
    /// </summary>
    public float CalculateSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            return 0f;
        
        // EN: Dot product calculation
        // ES: Cálculo del producto punto
        float dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
        
        // EN: Magnitude calculation using LINQ
        // ES: Cálculo de magnitud con LINQ
        float magnitude1 = (float)Math.Sqrt(vector1.Sum(v => v * v));
        float magnitude2 = (float)Math.Sqrt(vector2.Sum(v => v * v));
        
        if (magnitude1 == 0 || magnitude2 == 0)
            return 0f;
        
        return dotProduct / (magnitude1 * magnitude2);
    }
    
    /// <summary>
    /// EN: Find most relevant response using LINQ MaxBy.
    /// ES: Encontrar respuesta más relevante con LINQ MaxBy.
    /// </summary>
    private string FindMostRelevantResponse(float[] userEmbedding)
    {
        var bestMatch = responseDatabase
            .AsEnumerable()
            .Select(response => new 
            { 
                Response = response, 
                Similarity = CalculateSimilarity(userEmbedding, VectorizeInput(response)) 
            })
            .Where(x => x.Similarity >= similarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .FirstOrDefault();
        
        return bestMatch?.Response ?? "I didn't understand that.";
    }
    
    /// <summary>
    /// EN: Generate response asynchronously.
    /// ES: Generar respuesta de forma asincrónica.
    /// </summary>
    public async Task<string> GenerateResponseAsync(string userInput)
    {
        if (!isInitialized)
            return "System not initialized.";
        
        await Task.Delay(10); // EN: Simulate processing delay
        
        float[] userEmbedding = VectorizeInput(userInput);
        return FindMostRelevantResponse(userEmbedding);
    }
}
