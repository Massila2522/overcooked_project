using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public RecipeManager recipeManager;

    // Valeurs fixes - 6 recettes en 2 minutes
    private const int OBJECTIF_RECETTES = 6;
    private const float DUREE_SIMULATION = 120f; // 2 minutes
    
    private int totalRecipesServed = 0;
    private float gameStartTime;
    private bool gameStarted = false;
    private bool gameFinished = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (recipeManager == null)
        {
            recipeManager = FindFirstObjectByType<RecipeManager>();
        }
        
        // Reset
        totalRecipesServed = 0;
        gameFinished = false;
        gameStarted = true;
        gameStartTime = Time.time;
        
        Debug.Log("╔══════════════════════════════════════════════╗");
        Debug.Log("║   🎮 SIMULATION: 6 recettes en 2 minutes!    ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
    }

    private void Update()
    {
        if (!gameStarted || gameFinished) return;
        
        float elapsed = Time.time - gameStartTime;
        
        // Arrêter à exactement 2 minutes (120 secondes)
        if (elapsed >= DUREE_SIMULATION)
        {
            FinirSimulation();
        }
    }

    public void OnRecipeServed()
    {
        if (gameFinished) return;
        if (totalRecipesServed >= OBJECTIF_RECETTES) return;
        
        totalRecipesServed++;
        
        float elapsed = Time.time - gameStartTime;
        int min = Mathf.FloorToInt(elapsed / 60f);
        int sec = Mathf.FloorToInt(elapsed % 60f);
        
        Debug.Log($"✓ Recette {totalRecipesServed}/{OBJECTIF_RECETTES} - Temps: {min}:{sec:D2}");
        
        // Arrêter quand on atteint 6 recettes
        if (totalRecipesServed >= OBJECTIF_RECETTES)
        {
            FinirSimulation();
        }
    }
    
    private void FinirSimulation()
    {
        if (gameFinished) return;
        gameFinished = true;
        
        float elapsed = Time.time - gameStartTime;
        int min = Mathf.FloorToInt(elapsed / 60f);
        int sec = Mathf.FloorToInt(elapsed % 60f);
        
        Debug.Log("╔═══════════════════════════════════════════════════╗");
        if (totalRecipesServed >= OBJECTIF_RECETTES)
        {
            Debug.Log($"║   🏆 6/6 RECETTES EN {min}:{sec:D2}! 🏆               ║");
        }
        else
        {
            Debug.Log("║         ⏱ TEMPS ÉCOULÉ - 2 MINUTES ⏱              ║");
        }
        Debug.Log("╠═══════════════════════════════════════════════════╣");
        Debug.Log($"║   Recettes: {totalRecipesServed}/{OBJECTIF_RECETTES}                                 ║");
        Debug.Log($"║   Temps: {min}:{sec:D2}                                   ║");
        Debug.Log("╚═══════════════════════════════════════════════════╝");
        
        if (recipeManager != null)
        {
            recipeManager.StopSpawning();
        }
    }

    public int GetTotalRecipesServed()
    {
        return totalRecipesServed;
    }
    
    public bool IsGameFinished()
    {
        return gameFinished;
    }
    
    public int GetMaxRecipes()
    {
        return OBJECTIF_RECETTES;
    }

    public float GetElapsedTime()
    {
        if (!gameStarted) return 0f;
        if (gameFinished) return DUREE_SIMULATION;
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
}
