using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public RecipeManager recipeManager;
    
    private int totalRecipesServed = 0;
    private float gameStartTime;
    private bool gameStarted = false;

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
        
        // Démarrer le compteur de temps
        gameStartTime = Time.time;
        gameStarted = true;
    }

    public void OnRecipeServed()
    {
        totalRecipesServed++;
        Debug.Log($"Recette servie ! Total : {totalRecipesServed}");
    }

    public int GetTotalRecipesServed()
    {
        return totalRecipesServed;
    }

    public float GetElapsedTime()
    {
        if (!gameStarted) return 0f;
        return Time.time - gameStartTime;
    }

    public string GetFormattedTime()
    {
        float elapsed = GetElapsedTime();
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        int seconds = Mathf.FloorToInt(elapsed % 60f);
        return $"{minutes}:{seconds:D2}";
    }
}

