using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Hierarchical decision tree system with conditional branching and LINQ-based evaluation for narrative branching.
/// ES: Sistema de árbol de decisión jerárquico con ramificación condicional y evaluación basada en LINQ para ramificación narrativa.
/// </summary>
public class DecisionTreeManager : MonoBehaviour
{
    [System.Serializable]
    public class BranchCondition
    {
        public string variableName;
        public ComparisonOperator comparisonType;
        public int intValue;
        public float floatValue;
        public string stringValue;

        public enum ComparisonOperator { Equal, Greater, Less, NotEqual, GreaterOrEqual, LessOrEqual }
    }

    [System.Serializable]
    public class BranchNode
    {
        public int nodeId;
        [TextArea(2, 4)]
        public string description;
        public List<BranchCondition> conditions = new List<BranchCondition>();
        public UnityEngine.Events.UnityEvent onNodeSelected;
    }

    [SerializeField]
    [Tooltip("Lista de nodos de decisión en el árbol")]
    private List<BranchNode> decisionTree = new List<BranchNode>();

    [SerializeField]
    [Tooltip("Habilitar caché de evaluación para mejor rendimiento")]
    private bool cacheEnabled = true;

    private Dictionary<string, object> gameVariables = new Dictionary<string, object>();
    private Dictionary<int, bool> evaluationCache = new Dictionary<int, bool>();
    private int currentNodeId = -1;

    private void Start()
    {
        // EN: Initialize variables / ES: Inicializar variables
        gameVariables.Clear();
    }

    /// <summary>
    /// EN: Set a game variable that affects branch evaluation.
    /// ES: Establece una variable del juego que afecta la evaluación de rama.
    /// </summary>
    public void SetVariable(string key, object value)
    {
        gameVariables[key] = value;

        // EN: Invalidate cache on state change / ES: Invalidar caché al cambiar estado
        if (cacheEnabled)
            evaluationCache.Clear();
    }

    /// <summary>
    /// EN: Evaluate all branches and return matching node ID using LINQ filtering.
    /// ES: Evalúa todas las ramas y devuelve el ID de nodo coincidente utilizando filtrado LINQ.
    /// </summary>
    public int EvaluateBranches()
    {
        // EN: Use LINQ to find first matching branch / ES: Usar LINQ para encontrar primera rama coincidente
        var matchingBranch = decisionTree
            .Where(branch => EvaluateBranch(branch.nodeId))
            .FirstOrDefault();

        return matchingBranch?.nodeId ?? -1;
    }

    /// <summary>
    /// EN: Evaluate a specific branch's conditions.
    /// ES: Evalúa las condiciones de una rama específica.
    /// </summary>
    private bool EvaluateBranch(int branchId)
    {
        // EN: Check cache first / ES: Verificar caché primero
        if (cacheEnabled && evaluationCache.ContainsKey(branchId))
            return evaluationCache[branchId];

        var branch = decisionTree.FirstOrDefault(b => b.nodeId == branchId);
        if (branch == null)
            return false;

        // EN: All conditions must be true / ES: Todas las condiciones deben ser verdaderas
        bool allConditionsMet = branch.conditions.Count == 0 || 
            branch.conditions.All(condition => EvaluateCondition(condition));

        // EN: Cache result / ES: Resultado en caché
        if (cacheEnabled)
            evaluationCache[branchId] = allConditionsMet;

        return allConditionsMet;
    }

    /// <summary>
    /// EN: Evaluate a single condition against current game variables.
    /// ES: Evalúa una condición única contra las variables del juego actual.
    /// </summary>
    private bool EvaluateCondition(BranchCondition condition)
    {
        if (!gameVariables.ContainsKey(condition.variableName))
            return false;

        object varValue = gameVariables[condition.variableName];

        // EN: Type-safe condition evaluation / ES: Evaluación de condición type-safe
        if (varValue is int intVar)
        {
            return condition.comparisonType switch
            {
                BranchCondition.ComparisonOperator.Equal => intVar == condition.intValue,
                BranchCondition.ComparisonOperator.Greater => intVar > condition.intValue,
                BranchCondition.ComparisonOperator.Less => intVar < condition.intValue,
                BranchCondition.ComparisonOperator.NotEqual => intVar != condition.intValue,
                BranchCondition.ComparisonOperator.GreaterOrEqual => intVar >= condition.intValue,
                BranchCondition.ComparisonOperator.LessOrEqual => intVar <= condition.intValue,
                _ => false
            };
        }
        else if (varValue is float floatVar)
        {
            return condition.comparisonType switch
            {
                BranchCondition.ComparisonOperator.Equal => Mathf.Approximately(floatVar, condition.floatValue),
                BranchCondition.ComparisonOperator.Greater => floatVar > condition.floatValue,
                BranchCondition.ComparisonOperator.Less => floatVar < condition.floatValue,
                BranchCondition.ComparisonOperator.NotEqual => !Mathf.Approximately(floatVar, condition.floatValue),
                BranchCondition.ComparisonOperator.GreaterOrEqual => floatVar >= condition.floatValue,
                BranchCondition.ComparisonOperator.LessOrEqual => floatVar <= condition.floatValue,
                _ => false
            };
        }
        else if (varValue is string stringVar && condition.comparisonType == BranchCondition.ComparisonOperator.Equal)
        {
            return stringVar == condition.stringValue;
        }

        return false;
    }

    /// <summary>
    /// EN: Execute a branch by ID, triggering its callback event.
    /// ES: Ejecuta una rama por ID, activando su evento de callback.
    /// </summary>
    public void ExecuteBranch(int branchId)
    {
        var branch = decisionTree.FirstOrDefault(b => b.nodeId == branchId);
        if (branch != null)
        {
            currentNodeId = branchId;
            Debug.Log($"Branch executed: [{branchId}] {branch.description}");
            branch.onNodeSelected?.Invoke();
        }
    }

    /// <summary>
    /// EN: Get all available branches that match current conditions.
    /// ES: Obtiene todas las ramas disponibles que coinciden con las condiciones actuales.
    /// </summary>
    public List<BranchNode> GetAvailableBranches()
    {
        // EN: LINQ to filter available branches / ES: LINQ para filtrar ramas disponibles
        return decisionTree
            .Where(branch => EvaluateBranch(branch.nodeId))
            .ToList();
    }

    public int GetCurrentNodeId() => currentNodeId;

    public void ClearVariables() => gameVariables.Clear();
}
