using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public RecipeManager recipeManager;
    
    private int totalRecipesServed = 0;

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
}

