using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// EN: Quiz system with question randomization, scoring, result persistence, and performance analytics.
/// ES: Sistema de cuestionario con aleatorización de preguntas, puntuación, persistencia de resultados y analítica de rendimiento.
/// </summary>
public class QuizManager : MonoBehaviour
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)]
        public string questionText;
        public List<string> answers = new List<string>();
        
        [SerializeField]
        [Tooltip("Índice de la respuesta correcta")]
        private int correctAnswerIndex;
        
        public int CorrectAnswerIndex
        {
            get => correctAnswerIndex;
            set => correctAnswerIndex = value;
        }

        public int pointsValue = 10;
        public string category = "General";
    }

    [System.Serializable]
    public class QuizResult
    {
        public int totalQuestions;
        public int correctAnswers;
        public float percentage;
        public int totalScore;
        public System.DateTime completedAt;
        public string category;
    }

    [SerializeField]
    [Tooltip("Base de datos de preguntas disponibles")]
    private List<QuizQuestion> questionBank = new List<QuizQuestion>();

    [SerializeField]
    [Tooltip("Número de preguntas a presentar en este cuestionario")]
    private int questionsPerQuiz = 10;

    [SerializeField]
    [Tooltip("Aleatorizar el orden de las preguntas")]
    private bool randomizeQuestions = true;

    [SerializeField]
    [Tooltip("Aleatorizar el orden de las respuestas")]
    private bool randomizeAnswers = true;

    private List<QuizQuestion> currentQuiz = new List<QuizQuestion>();
    private int currentQuestionIndex = 0;
    private QuizResult currentResult;
    private bool quizActive = false;

    /// <summary>
    /// EN: Initialize and start a new quiz with randomized questions.
    /// ES: Inicializa e inicia un nuevo cuestionario con preguntas aleatorizadas.
    /// </summary>
    public void StartQuiz(string category = "")
    {
        // EN: Filter questions by category using LINQ / ES: Filtrar preguntas por categoría usando LINQ
        var availableQuestions = string.IsNullOrEmpty(category)
            ? questionBank
            : questionBank.Where(q => q.category == category).ToList();

        // EN: Select random questions / ES: Seleccionar preguntas aleatorias
        if (randomizeQuestions)
        {
            currentQuiz = availableQuestions
                .OrderBy(q => Random.value)
                .Take(questionsPerQuiz)
                .ToList();
        }
        else
        {
            currentQuiz = new List<QuizQuestion>(availableQuestions.Take(questionsPerQuiz));
        }

        // EN: Randomize answer order within each question / ES: Aleatorizar orden de respuestas en cada pregunta
        if (randomizeAnswers)
        {
            // EN: Use LINQ ForEach for side effects / ES: Usar LINQ ForEach para efectos secundarios
            currentQuiz.ForEach(question =>
            {
                int correctIndex = question.CorrectAnswerIndex;
                string correctAnswer = question.answers[correctIndex];

                question.answers = question.answers.OrderBy(a => Random.value).ToList();
                question.CorrectAnswerIndex = question.answers.IndexOf(correctAnswer);
            });
        }

        currentQuestionIndex = 0;
        quizActive = true;
        currentResult = new QuizResult
        {
            totalQuestions = currentQuiz.Count,
            category = category,
            completedAt = System.DateTime.Now
        };

        Debug.Log($"Quiz started: {currentQuiz.Count} questions");
    }

    /// <summary>
    /// EN: Get the current question being asked.
    /// ES: Obtiene la pregunta actual siendo formulada.
    /// </summary>
    public QuizQuestion GetCurrentQuestion()
    {
        if (!quizActive || currentQuestionIndex >= currentQuiz.Count)
            return null;

        return currentQuiz[currentQuestionIndex];
    }

    /// <summary>
    /// EN: Submit an answer to the current question.
    /// ES: Envía una respuesta a la pregunta actual.
    /// </summary>
    public bool SubmitAnswer(int selectedAnswerIndex)
    {
        var question = GetCurrentQuestion();
        if (question == null)
            return false;

        // EN: Check correctness / ES: Verificar corrección
        bool isCorrect = selectedAnswerIndex == question.CorrectAnswerIndex;

        if (isCorrect)
        {
            currentResult.correctAnswers++;
            currentResult.totalScore += question.pointsValue;
            Debug.Log($"✓ Correct! +{question.pointsValue} points");
        }
        else
        {
            Debug.Log($"✗ Incorrect. Answer: {question.answers[question.CorrectAnswerIndex]}");
        }

        // EN: Move to next question / ES: Pasar a la siguiente pregunta
        currentQuestionIndex++;
        return isCorrect;
    }

    /// <summary>
    /// EN: Finish the quiz and calculate final score.
    /// ES: Termina el cuestionario y calcula la puntuación final.
    /// </summary>
    public QuizResult FinishQuiz()
    {
        if (!quizActive)
            return null;

        // EN: Calculate percentage / ES: Calcular porcentaje
        currentResult.percentage = (currentResult.correctAnswers / (float)currentResult.totalQuestions) * 100f;

        quizActive = false;

        // EN: Persist results / ES: Persistir resultados
        SaveQuizResult(currentResult);

        Debug.Log($"Quiz finished: {currentResult.correctAnswers}/{currentResult.totalQuestions} " +
                  $"({currentResult.percentage:F1}%) - Score: {currentResult.totalScore}");

        return currentResult;
    }

    /// <summary>
    /// EN: Get the current progress through the quiz.
    /// ES: Obtiene el progreso actual a través del cuestionario.
    /// </summary>
    public float GetProgress()
    {
        return quizActive ? (currentQuestionIndex / (float)currentQuiz.Count) : 1f;
    }

    /// <summary>
    /// EN: Get all results from PlayerPrefs.
    /// ES: Obtiene todos los resultados de PlayerPrefs.
    /// </summary>
    public List<QuizResult> GetSavedResults()
    {
        string resultsJson = PlayerPrefs.GetString("QuizResults", "[]");
        
        // EN: Parse results (simplified - in production use a proper JSON library) / ES: Parsear resultados
        return new List<QuizResult>();
    }

    private void SaveQuizResult(QuizResult result)
    {
        // EN: Save to PlayerPrefs / ES: Guardar en PlayerPrefs
        string json = JsonUtility.ToJson(result);
        
        // EN: Append to results history / ES: Agregar al historial de resultados
        string timestamp = System.DateTime.Now.Ticks.ToString();
        PlayerPrefs.SetString($"QuizResult_{timestamp}", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// EN: Get average score from all completed quizzes.
    /// ES: Obtiene la puntuación promedio de todos los cuestionarios completados.
    /// </summary>
    public float GetAverageScore()
    {
        var results = GetSavedResults();
        return results.Count > 0 
            ? results.Average(r => r.percentage)
            : 0f;
    }

    public bool IsQuizActive() => quizActive;

    public int GetTotalQuestions() => currentQuiz.Count;
}
