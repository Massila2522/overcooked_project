using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecipeManager : MonoBehaviour
{
    [Header("Recipe Settings")]
    public float minDelay = 2f;
    public float maxDelay = 5f;

    private Queue<Recipe> recipeQueue = new Queue<Recipe>();
    private IngredientQueue ingredientQueue = new IngredientQueue();
    private int recipeOrder = 0;
    private object lockObject = new object();
    private bool isSpawning = true;

    private void Start()
    {
        StartCoroutine(SpawnRecipes());
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("[RecipeManager] Génération de recettes arrêtée.");
    }
    
    public bool IsSpawning()
    {
        return isSpawning;
    }

    private IEnumerator SpawnRecipes()
    {
        while (isSpawning)
        {
            // Délai aléatoire entre 2 et 5 secondes
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
            
            // Vérifier si on doit continuer
            if (!isSpawning) break;

            // Créer une recette aléatoire
            RecipeType randomType = GetRandomRecipeType();
            Recipe newRecipe = new Recipe(randomType, recipeOrder++);
            
            AddRecipe(newRecipe);
        }
    }

    private RecipeType GetRandomRecipeType()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0: return RecipeType.OnionSoup;
            case 1: return RecipeType.TomatoSoup;
            case 2: return RecipeType.MushroomSoup;
            case 3: return RecipeType.Burger;
            default: return RecipeType.Burger;
        }
    }

    public void AddRecipe(Recipe recipe)
    {
        lock (lockObject)
        {
            recipeQueue.Enqueue(recipe);
            
            // Ajouter les ingrédients nécessaires à la file d'ingrédients avec le recipeId
            foreach (IngredientType ingredientType in recipe.RequiredIngredients)
            {
                ingredientQueue.AddIngredient(ingredientType, recipe.Order);
            }
        }
    }

    public Recipe GetNextRecipe()
    {
        lock (lockObject)
        {
            if (recipeQueue.Count == 0)
            {
                return null;
            }
            // Retourner la première recette non réservée
            foreach (Recipe recipe in recipeQueue)
            {
                if (!recipe.IsReserved)
                {
                    return recipe;
                }
            }
            return null; // Toutes les recettes sont réservées
        }
    }

    public bool TryReserveRecipe(Recipe recipe, Agent agent)
    {
        lock (lockObject)
        {
            if (recipe == null || recipe.IsReserved)
            {
                return false;
            }
            
            // Vérifier que la recette est toujours dans la file
            if (!recipeQueue.Contains(recipe))
            {
                return false;
            }
            
            recipe.IsReserved = true;
            recipe.ReservedBy = agent;
            Debug.Log($"Recette {recipe.GetDisplayName()} réservée par {agent.gameObject.name}");
            return true;
        }
    }

    public void CompleteRecipe(Recipe recipe)
    {
        lock (lockObject)
        {
            if (recipeQueue.Count == 0)
            {
                return;
            }
            
            // Retirer la recette de la file
            Queue<Recipe> newQueue = new Queue<Recipe>();
            bool recipeCompleted = false;
            while (recipeQueue.Count > 0)
            {
                Recipe r = recipeQueue.Dequeue();
                if (r.Order == recipe.Order)
                {
                    Debug.Log($"Recette {r.GetDisplayName()} complétée et retirée de la file");
                    recipeCompleted = true;
                }
                else
                {
                    newQueue.Enqueue(r);
                }
            }
            recipeQueue = newQueue;
            // OnRecipeServed() est appelé dans ServeStation.cs
        }
    }

    public IngredientQueueItem GetNextNeededIngredient()
    {
        return ingredientQueue.TakeIngredient();
    }

    public int GetRecipeCount()
    {
        return recipeQueue.Count;
    }

    public int GetIngredientQueueCount()
    {
        return ingredientQueue.Count();
    }

    public List<Recipe> GetAllRecipes()
    {
        lock (lockObject)
        {
            return new List<Recipe>(recipeQueue);
        }
    }
}

