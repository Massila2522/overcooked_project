using UnityEngine;
using System.Collections;

/// <summary>
/// GameManager amélioré avec meilleure gestion des états de jeu,
/// compte à rebours de démarrage et effets visuels.
/// </summary>
public class ImprovedGameManager : MonoBehaviour
{
    public static ImprovedGameManager Instance { get; private set; }

    public enum GameState
    {
        Countdown,      // Compte à rebours avant démarrage
        Playing,        // Jeu en cours
        Paused,         // Jeu en pause
        Victory,        // Victoire - objectif atteint
        TimeUp          // Temps écoulé (si limite de temps)
    }

    [Header("Configuration du Jeu")]
    [SerializeField] private int targetRecipes = 5;
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private bool hasTimeLimit = false;
    [SerializeField] private float timeLimitSeconds = 300f; // 5 minutes par défaut

    [Header("Références")]
    [SerializeField] private RecipeManager recipeManager;

    // État du jeu
    private GameState currentState = GameState.Countdown;
    private float gameStartTime;
    private float gameEndTime;
    private int recipesCompleted = 0;
    private float countdownRemaining;

    // Événements
    public delegate void GameStateChanged(GameState newState);
    public event GameStateChanged OnGameStateChanged;

    public delegate void RecipeCompleted(int completed, int target);
    public event RecipeCompleted OnRecipeCompleted;

    public delegate void CountdownTick(int secondsRemaining);
    public event CountdownTick OnCountdownTick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (recipeManager == null)
        {
            recipeManager = FindFirstObjectByType<RecipeManager>();
        }

        // Démarrer avec le compte à rebours
        StartCoroutine(CountdownSequence());
    }

    private void Update()
    {
        if (currentState == GameState.Playing && hasTimeLimit)
        {
            float elapsed = GetElapsedTime();
            if (elapsed >= timeLimitSeconds)
            {
                EndGame(GameState.TimeUp);
            }
        }
    }

    private IEnumerator CountdownSequence()
    {
        currentState = GameState.Countdown;
        OnGameStateChanged?.Invoke(currentState);

        countdownRemaining = countdownDuration;

        Debug.Log("╔════════════════════════════════════════════╗");
        Debug.Log("║     🎮 PRÉPARATION AU DÉMARRAGE 🎮         ║");
        Debug.Log($"║     Objectif: {targetRecipes} recettes à servir          ║");
        Debug.Log("╚════════════════════════════════════════════╝");

        while (countdownRemaining > 0)
        {
            int seconds = Mathf.CeilToInt(countdownRemaining);
            OnCountdownTick?.Invoke(seconds);
            Debug.Log($"⏳ Démarrage dans {seconds}...");
            
            yield return new WaitForSeconds(1f);
            countdownRemaining -= 1f;
        }

        StartGame();
    }

    private void StartGame()
    {
        currentState = GameState.Playing;
        gameStartTime = Time.time;
        recipesCompleted = 0;

        OnGameStateChanged?.Invoke(currentState);

        Debug.Log("╔════════════════════════════════════════════╗");
        Debug.Log("║          🚀 C'EST PARTI! 🚀                ║");
        Debug.Log($"║   Servez {targetRecipes} recettes le plus vite possible!  ║");
        Debug.Log("╚════════════════════════════════════════════╝");

        // Activer la génération de recettes
        if (recipeManager != null)
        {
            // Le RecipeManager démarre automatiquement dans Start()
        }
    }

    /// <summary>
    /// Appelé quand une recette est servie avec succès.
    /// </summary>
    public void OnRecipeServed()
    {
        if (currentState != GameState.Playing) return;

        recipesCompleted++;
        
        Debug.Log($"✅ Recette {recipesCompleted}/{targetRecipes} servie! Temps: {GetFormattedTime()}");
        
        OnRecipeCompleted?.Invoke(recipesCompleted, targetRecipes);

        // Vérifier la victoire
        if (recipesCompleted >= targetRecipes)
        {
            EndGame(GameState.Victory);
        }
    }

    private void EndGame(GameState endState)
    {
        if (currentState != GameState.Playing) return;

        currentState = endState;
        gameEndTime = Time.time;

        OnGameStateChanged?.Invoke(currentState);

        // Arrêter la génération de recettes
        if (recipeManager != null)
        {
            recipeManager.StopSpawning();
        }

        // Afficher les résultats
        if (endState == GameState.Victory)
        {
            ShowVictoryScreen();
        }
        else if (endState == GameState.TimeUp)
        {
            ShowTimeUpScreen();
        }
    }

    private void ShowVictoryScreen()
    {
        string finalTime = GetFormattedTimeWithMs();
        
        Debug.Log("╔══════════════════════════════════════════════════╗");
        Debug.Log("║           🏆 FÉLICITATIONS! 🏆                   ║");
        Debug.Log("╠══════════════════════════════════════════════════╣");
        Debug.Log($"║   ✓ {targetRecipes} recettes servies avec succès!          ║");
        Debug.Log($"║   ⏱ Temps final: {finalTime}                    ║");
        Debug.Log("╠══════════════════════════════════════════════════╣");
        
        // Statistiques des agents
        CooperativeAgent[] agents = FindObjectsByType<CooperativeAgent>(FindObjectsSortMode.None);
        Debug.Log($"║   👥 {agents.Length} agents ont travaillé en équipe:       ║");
        foreach (var agent in agents)
        {
            Debug.Log($"║      - Agent {agent.GetAgentId() + 1}: {agent.GetRecipesCompleted()} recettes      ║");
        }
        
        Debug.Log("╚══════════════════════════════════════════════════╝");
    }

    private void ShowTimeUpScreen()
    {
        Debug.Log("╔══════════════════════════════════════════════════╗");
        Debug.Log("║           ⏰ TEMPS ÉCOULÉ! ⏰                    ║");
        Debug.Log("╠══════════════════════════════════════════════════╣");
        Debug.Log($"║   Recettes servies: {recipesCompleted}/{targetRecipes}                    ║");
        Debug.Log("║   Essayez encore pour battre votre record!       ║");
        Debug.Log("╚══════════════════════════════════════════════════╝");
    }

    // ============================================
    // GETTERS PUBLICS
    // ============================================

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    public bool IsGameFinished()
    {
        return currentState == GameState.Victory || currentState == GameState.TimeUp;
    }

    public int GetTotalRecipesServed()
    {
        return recipesCompleted;
    }

    public int GetMaxRecipes()
    {
        return targetRecipes;
    }

    public float GetElapsedTime()
    {
        if (currentState == GameState.Countdown) return 0f;
        if (IsGameFinished()) return gameEndTime - gameStartTime;
        return Time.time - gameStartTime;
    }

    public string GetFormattedTime()
    {
        float elapsed = GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        return $"{minutes}:{seconds:D2}";
    }

    public string GetFormattedTimeWithMs()
    {
        float elapsed = GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        int milliseconds = Mathf.FloorToInt((elapsed % 1f) * 1000f);
        return $"{minutes}:{seconds:D2}.{milliseconds:D3}";
    }

    public float GetRemainingTime()
    {
        if (!hasTimeLimit) return float.MaxValue;
        return Mathf.Max(0, timeLimitSeconds - GetElapsedTime());
    }

    public float GetCountdownRemaining()
    {
        return countdownRemaining;
    }

    // ============================================
    // CONTRÔLES DU JEU
    // ============================================

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            OnGameStateChanged?.Invoke(currentState);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}

