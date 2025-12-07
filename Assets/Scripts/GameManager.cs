using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public RecipeManager recipeManager;

    // Durée fixe de 2 minutes - on compte les recettes
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
        Debug.Log("║   🎮 SIMULATION: 2 minutes!                  ║");
        Debug.Log("║   ⏱  Combien de recettes en 2 min?           ║");
        Debug.Log("╚══════════════════════════════════════════════╝");
    }

    private void Update()
    {
        // Le timer continue indéfiniment - on note juste les recettes à 2 minutes
        if (!gameStarted) return;
        
        float elapsed = Time.time - gameStartTime;
        
        // Afficher le résultat à 2 minutes (mais continuer)
        if (!gameFinished && elapsed >= DUREE_SIMULATION)
        {
            AfficherResultat2Minutes();
        }
    }

    public void OnRecipeServed()
    {
        totalRecipesServed++;
        
        float elapsed = Time.time - gameStartTime;
        int min = Mathf.FloorToInt(elapsed / 60f);
        int sec = Mathf.FloorToInt(elapsed % 60f);
        
        Debug.Log($"✓ Recette #{totalRecipesServed} - Temps: {min}:{sec:D2}");
    }
    
    private int recettesA2Minutes = 0;
    
    private void AfficherResultat2Minutes()
    {
        gameFinished = true; // Marque que les 2 min sont passées
        recettesA2Minutes = totalRecipesServed;
        
        Debug.Log("╔═══════════════════════════════════════════════════╗");
        Debug.Log("║         ⏱  2 MINUTES ÉCOULÉES! ⏱                 ║");
        Debug.Log("╠═══════════════════════════════════════════════════╣");
        Debug.Log($"║   🍽  RECETTES EN 2 MIN: {recettesA2Minutes}                       ║");
        Debug.Log("╠═══════════════════════════════════════════════════╣");
        Debug.Log("║   ▶  Le jeu continue...                           ║");
        Debug.Log("╚═══════════════════════════════════════════════════╝");
        
        // On ne stoppe PAS le recipeManager - le jeu continue
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
        return totalRecipesServed; // Pas de max, on compte juste
    }

    public float GetElapsedTime()
    {
        if (!gameStarted) return 0f;
        return Time.time - gameStartTime;  // Continue après 2 min
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
